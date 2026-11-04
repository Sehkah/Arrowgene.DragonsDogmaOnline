using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Arrowgene.Ddon.Cli.Command;
using Arrowgene.Ddon.Cli.Command.Packet;
using Arrowgene.Ddon.Server.Network;
using Arrowgene.Ddon.Shared.Network;
using Xunit;
using PacketSource = Arrowgene.Ddon.Shared.Network.PacketSource;

namespace Arrowgene.Ddon.Test.Server.Network
{
    /// <summary>
    /// Framing over the frames of a real capture, the fixture of PacketCommandTest (game connection, client 03040008).
    /// </summary>
    public class PacketFactoryReadTest
    {
        private static readonly byte[] CaptureKey = Encoding.ASCII.GetBytes("_24ERLGmXGC0NTivXeXKZ24McjaaHJEC");

        private static readonly List<byte[]> ClientFrames = CaptureFrames(PacketSource.Client);
        private static readonly List<byte[]> ServerFrames = CaptureFrames(PacketSource.Server);

        public static readonly TheoryData<string> Resolvers = new TheoryData<string> { "login", "game" };

        private static IPacketIdResolver Resolver(string name) => name == "login"
            ? PacketIdResolver.LoginPacketIdResolver
            : PacketIdResolver.GamePacketIdResolver;

        private static List<byte[]> CaptureFrames(PacketSource source)
        {
            string yaml = TestUtils.GetTestFileAsString("stream51-marked.pcapng_tcp-stream-11.yaml");
            List<PcapPacket> captured = new PacketCommand().ReadYamlPcap(yaml);
            byte[] stream = captured.Where(packet => packet.Source == source).SelectMany(packet => packet.Data).ToArray();

            List<byte[]> frames = new List<byte[]>();
            int position = 0;
            while (position + 2 <= stream.Length)
            {
                int size = 2 + ((stream[position] << 8) | stream[position + 1]);
                if (position + size > stream.Length)
                {
                    break;
                }

                frames.Add(stream.Skip(position).Take(size).ToArray());
                position += size;
            }

            return frames;
        }

        private static PacketFactory NewFactory(string resolver)
        {
            PacketFactory factory = new PacketFactory(Resolver(resolver));
            factory.SetCamelliaKey(CaptureKey);
            return factory;
        }

        private static string Print(IEnumerable<IPacket> packets) =>
            string.Join("|", packets.Select(packet => $"{packet.Id} {packet.Source} {packet.Count} {Convert.ToHexString(packet.Data)}"));

        /// <summary>A stretch of frames holding both a frame of the minimum 16 bytes and a longer one before and after it.</summary>
        private static byte[] Stretch(List<byte[]> frames)
        {
            int shortFrame = frames.FindIndex(2, frame => frame.Length == 18);
            Assert.True(shortFrame >= 2, "the capture has no 16-byte frame");
            return frames[0].Concat(frames.Skip(shortFrame - 1).Take(3).SelectMany(frame => frame)).ToArray();
        }

        [Theory]
        [MemberData(nameof(Resolvers))]
        public void A_split_at_any_offset_gives_the_same_packets(string resolver)
        {
            foreach (byte[] stream in new[] { Stretch(ClientFrames), Stretch(ServerFrames) })
            {
                string whole = Print(NewFactory(resolver).Read(stream));
                Assert.NotEqual("", whole);

                for (int offset = 1; offset < stream.Length; offset++)
                {
                    PacketFactory factory = NewFactory(resolver);
                    List<IPacket> packets = factory.Read(stream[..offset]);
                    packets.AddRange(factory.Read(stream[offset..]));

                    Assert.True(whole == Print(packets), $"split at {offset} of {stream.Length}");
                }
            }
        }

        [Theory]
        [MemberData(nameof(Resolvers))]
        public void A_frame_fed_one_byte_at_a_time_gives_the_same_packets(string resolver)
        {
            byte[] stream = Stretch(ClientFrames);
            string whole = Print(NewFactory(resolver).Read(stream));

            PacketFactory factory = NewFactory(resolver);
            List<IPacket> packets = new List<IPacket>();
            foreach (byte value in stream)
            {
                packets.AddRange(factory.Read(new[] { value }));
            }

            Assert.Equal(whole, Print(packets));
        }

        [Fact]
        public void A_split_after_the_length_prefix_of_a_16_byte_frame_keeps_the_prefix()
        {
            byte[] first = ClientFrames[0];
            byte[] shortFrame = ClientFrames.Skip(1).First(frame => frame.Length == 18);
            PacketFactory factory = NewFactory("game");

            List<IPacket> packets = factory.Read(first.Concat(shortFrame[..2]).ToArray());
            packets.AddRange(factory.Read(shortFrame[2..]));

            Assert.Equal(2, packets.Count);
        }

        [Fact]
        public void A_split_inside_the_next_length_prefix_keeps_the_byte()
        {
            byte[] first = ClientFrames[0];
            byte[] second = ClientFrames.First(frame => frame != first && frame.Length > 18);
            PacketFactory factory = NewFactory("game");

            // The previous frame has to be over 16 bytes for this to have rewound into its ciphertext.
            Assert.True(first.Length > 18);
            List<IPacket> packets = factory.Read(first.Concat(second[..1]).ToArray());
            Assert.Single(packets);
            packets.AddRange(factory.Read(second[1..]));

            Assert.Equal(2, packets.Count);
            Assert.Equal(PacketSource.Client, packets[1].Source);
        }

        /// <summary>Encrypt a frame with this source byte, the shape of a real packet, under the factory's fixed key.</summary>
        private static byte[] Frame(PacketFactory factory, byte source)
        {
            byte[] plain = new byte[32];
            plain[3] = 1;
            plain[4] = source;
            byte[] encrypted = factory.Encrypt(plain);
            return new[] { (byte)0, (byte)encrypted.Length }.Concat(encrypted).ToArray();
        }

        private static (RecordingClient Client, byte[] Good) ClientPastItsFirstPacket(string resolver)
        {
            PacketFactory factory = new PacketFactory(Resolver(resolver));
            RecordingClient client = new RecordingClient(factory);
            byte[] first = Frame(factory, 0);
            Assert.Single(client.Receive(first));
            return (client, Frame(factory, (byte)PacketSource.Client));
        }

        [Theory]
        [MemberData(nameof(Resolvers))]
        public void Sources_0_and_0x34_are_accepted(string resolver)
        {
            (RecordingClient client, byte[] good) = ClientPastItsFirstPacket(resolver);
            byte[] server = Frame(new PacketFactory(Resolver(resolver)), (byte)PacketSource.Server);

            Assert.Equal(2, client.Receive(good.Concat(server).ToArray()).Count);
            Assert.Equal(0, client.Closes);
        }

        public static readonly TheoryData<string, int> BadLengths = new TheoryData<string, int>
        {
            { "login", 0 }, { "game", 0 },
            { "login", 15 }, { "game", 15 },
            { "login", 17 }, { "game", 17 },
            { "game", 65535 },
        };

        [Theory]
        [MemberData(nameof(BadLengths))]
        public void A_bad_frame_length_closes_and_keeps_what_was_decoded(string resolver, int length)
        {
            (RecordingClient client, byte[] good) = ClientPastItsFirstPacket(resolver);
            byte[] bad = new byte[2 + length];
            bad[0] = (byte)((length >> 8) & 0xFF);
            bad[1] = (byte)(length & 0xFF);

            List<IPacket> packets = client.Receive(good.Concat(bad).ToArray());

            Assert.Single(packets);
            Assert.Equal(1, client.Closes);
        }

        [Theory]
        [MemberData(nameof(Resolvers))]
        public void A_bad_source_byte_closes_and_keeps_what_was_decoded(string resolver)
        {
            (RecordingClient client, byte[] good) = ClientPastItsFirstPacket(resolver);
            byte[] bad = Frame(new PacketFactory(Resolver(resolver)), 0x12);

            List<IPacket> packets = client.Receive(good.Concat(bad).ToArray());

            Assert.Single(packets);
            Assert.Equal(1, client.Closes);
        }

        [Fact]
        public void Nothing_is_read_after_a_bad_frame()
        {
            (RecordingClient client, byte[] good) = ClientPastItsFirstPacket("game");

            client.Receive(new byte[] { 0, 17 });

            Assert.Empty(client.Receive(good));
            Assert.Equal(1, client.Closes);
        }
    }
}
