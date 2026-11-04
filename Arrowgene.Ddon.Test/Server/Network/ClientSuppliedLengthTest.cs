using System;
using System.IO;
using System.Linq;
using Arrowgene.Ddon.Server;
using Arrowgene.Ddon.Server.Network;
using Arrowgene.Ddon.Shared.Entity;
using Arrowgene.Ddon.Shared.Entity.PacketStructure;
using Arrowgene.Ddon.Shared.Entity.Structure;
using Arrowgene.Ddon.Shared.Network;
using Arrowgene.Logging;
using Xunit;
using PacketSource = Arrowgene.Ddon.Shared.Network.PacketSource;

namespace Arrowgene.Ddon.Test.Server.Network
{
    public class ClientSuppliedLengthTest
    {
        /// <summary>The three C2S readers, each from its bytes after the prefix fields up to its s32 length.</summary>
        public static readonly TheoryData<string> Readers = new TheoryData<string> { "lobby", "party", "partyAll" };

        private static byte[] Body(string reader, byte[] length, byte[] data)
        {
            byte[] prefix = reader switch
            {
                "lobby" => new byte[] { 1, 0, 0, 0, 7 }, // Type, CharacterId
                "party" => new byte[] { 0, 0, 0, 0 }, // An empty CharacterIdList
                _ => Array.Empty<byte>(),
            };
            return prefix.Concat(length).Concat(data).ToArray();
        }

        private static byte[] Read(string reader, byte[] body) => reader switch
        {
            "lobby" => EntitySerializer.Get<C2SLobbyLobbyDataMsgReq>().Read(body).RpcPacket,
            "party" => EntitySerializer.Get<C2SPartySendBinaryMsgNtc>().Read(body).Data,
            _ => EntitySerializer.Get<C2SPartySendBinaryMsgAllNtc>().Read(body).Data,
        };

        [Theory]
        [MemberData(nameof(Readers))]
        public void A_length_that_fits_reads_and_leaves_the_padding(string reader)
        {
            byte[] body = Body(reader, new byte[] { 0, 0, 0, 2 }, new byte[] { 0xAA, 0xBB, 0, 0, 0 });

            Assert.Equal(new byte[] { 0xAA, 0xBB }, Read(reader, body));
        }

        [Theory]
        [MemberData(nameof(Readers))]
        public void A_negative_length_is_a_short_packet(string reader)
        {
            byte[] body = Body(reader, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, new byte[8]);

            Assert.Throws<EndOfStreamException>(() => Read(reader, body));
        }

        [Theory]
        [MemberData(nameof(Readers))]
        public void A_huge_length_is_a_short_packet(string reader)
        {
            byte[] body = Body(reader, new byte[] { 0x7F, 0xFF, 0xFF, 0xFF }, new byte[8]);

            Assert.Throws<EndOfStreamException>(() => Read(reader, body));
        }

        [Theory]
        [MemberData(nameof(Readers))]
        public void A_length_past_the_end_is_a_short_packet(string reader)
        {
            byte[] body = Body(reader, new byte[] { 0, 0, 0, 9 }, new byte[8]);

            Assert.Throws<EndOfStreamException>(() => Read(reader, body));
        }

        [Fact]
        public void A_large_password_size_can_be_written()
        {
            byte[] password = new byte[200];

            byte[] game = EntitySerializer.Get<S2CCertClientChallengeRes>().Write(new S2CCertClientChallengeRes { PasswordEncSize = 200, PasswordEnc = password });
            byte[] login = EntitySerializer.Get<L2CClientChallengeRes>().Write(new L2CClientChallengeRes { PasswordEncSize = 200, PasswordEnc = password });

            Assert.Equal(8 + 2 + 200, game.Length);
            Assert.Equal(8 + 2 + 200, login.Length);
        }

        [Fact]
        public void A_real_password_size_writes_the_block_of_62()
        {
            byte[] password = new byte[16];

            byte[] game = EntitySerializer.Get<S2CCertClientChallengeRes>().Write(new S2CCertClientChallengeRes { PasswordEncSize = 16, PasswordEnc = password });

            Assert.Equal(8 + 2 + 62, game.Length);
        }

        private class LoggingClient : RecordingClient
        {
            public LoggingClient(PacketFactory factory) : base(factory)
            {
            }
        }

        [Fact]
        public void Logging_a_malformed_packet_does_not_escape_receive()
        {
            // A logger per client type: this setting reaches no other test.
            LogProvider.Logger<ServerLogger>(typeof(LoggingClient)).Configure(null, new ServerSetting
            {
                LogIncomingPackets = true,
                LogIncomingPacketStructure = true,
            });
            PacketFactory factory = new PacketFactory(PacketIdResolver.GamePacketIdResolver);
            LoggingClient client = new LoggingClient(factory);

            byte[] Frame(byte group, byte sub)
            {
                byte[] plain = new byte[32];
                plain[0] = group;
                plain[3] = sub;
                plain[4] = (byte)PacketSource.Client;
                byte[] encrypted = factory.Encrypt(plain);
                return new[] { (byte)0, (byte)encrypted.Length }.Concat(encrypted).ToArray();
            }

            client.Receive(Frame(0, 1)); // The first packet of a connection has no header.

            // 44.0.1 reads 256 bytes and more, so these 23 of them run out.
            IPacket packet = Assert.Single(client.Receive(Frame(44, 1)));
            Assert.Equal(PacketId.C2S_CERT_CLIENT_CHALLENGE_REQ, packet.Id);
            Assert.Equal(0, client.Closes);
        }

        [Fact]
        public void The_largest_body_that_fits_the_u16_length_is_sent()
        {
            RecordingClient client = new RecordingClient();

            client.Send(new Packet(PacketId.S2C_CONNECTION_PING_RES, new byte[65510]));

            Assert.Equal(65520, Assert.Single(client.Frames).Plain.Length);
        }

        [Theory]
        [InlineData(65511)] // Padding takes it past the u16.
        [InlineData(65524)]
        [InlineData(65525)] // Over the limit before padding too.
        [InlineData(70000)]
        public void A_body_over_the_u16_length_is_not_sent(int size)
        {
            RecordingClient client = new RecordingClient();

            Exception thrown = Record.Exception(() => client.Send(new Packet(PacketId.S2C_CONNECTION_PING_RES, new byte[size])));

            Assert.Null(thrown);
            Assert.Empty(client.Frames);
        }

        [Fact]
        public void A_notice_over_the_limit_uses_no_counter()
        {
            PacketFactory factory = new PacketFactory(PacketIdResolver.GamePacketIdResolver);
            PacketId notice = PacketId.S2C_CONNECTION_INFORMATION_NTC;

            Assert.Null(factory.Write(new Packet(notice, new byte[65511])));

            Packet next = new Packet(notice, Array.Empty<byte>());
            factory.Write(next);
            Assert.Equal(1u, next.Count);
        }
    }
}
