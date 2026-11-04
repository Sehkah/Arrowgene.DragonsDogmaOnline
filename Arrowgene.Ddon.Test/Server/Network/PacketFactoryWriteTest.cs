using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Arrowgene.Ddon.Server.Network;
using Arrowgene.Ddon.Shared.Network;
using Xunit;

namespace Arrowgene.Ddon.Test.Server.Network
{
    /// <summary>
    /// A client's sends come from its lane and from timers, so they can overlap on one factory.
    /// </summary>
    public class PacketFactoryWriteTest
    {
        private const int Threads = 4;
        private const int FramesPerThread = 3000;
        private const int BodySize = 247;
        private const int HeaderSize = 9;

        private static readonly byte[] Key = Encoding.ASCII.GetBytes("0123456789abcdef0123456789ABCDEF");

        private static PacketFactory NewFactory()
        {
            PacketFactory factory = new PacketFactory(PacketIdResolver.GamePacketIdResolver);
            factory.SetCamelliaKey(Key);
            return factory;
        }

        private static byte[] Body(int thread, int frame)
        {
            byte[] body = new byte[BodySize];
            for (int i = 0; i < body.Length; i++)
            {
                body[i] = unchecked((byte)(thread * 31 + frame * 7 + i));
            }

            body[0] = unchecked((byte)thread);
            body[1] = unchecked((byte)(frame >> 8));
            body[2] = unchecked((byte)frame);
            return body;
        }

        private static byte[] Decrypt(PacketFactory factory, byte[] frame) => factory.Decrypt(frame[2..]);

        private static uint Count(byte[] plain) => ((uint)plain[5] << 24) | ((uint)plain[6] << 16) | ((uint)plain[7] << 8) | plain[8];

        [Fact]
        public void Frames_written_on_several_threads_each_decrypt_to_what_was_written()
        {
            byte[][][] frames = new byte[Threads][][];
            Barrier start = new Barrier(Threads);
            PacketFactory factory = NewFactory();
            Thread[] threads = Enumerable.Range(0, Threads).Select(t => new Thread(() =>
            {
                frames[t] = new byte[FramesPerThread][];
                start.SignalAndWait();
                for (int i = 0; i < FramesPerThread; i++)
                {
                    // Every other frame is an NTC, the kind that takes a count.
                    PacketId id = i % 2 == 0 ? PacketId.S2C_ACHIEVEMENT_ACHIEVEMENT_COMPLETE_NTC : PacketId.S2C_CONNECTION_PING_RES;
                    frames[t][i] = factory.Write(new Packet(id, Body(t, i)));
                }
            })).ToArray();

            foreach (Thread thread in threads)
            {
                thread.Start();
            }

            foreach (Thread thread in threads)
            {
                thread.Join();
            }

            PacketFactory reader = NewFactory();
            int corrupted = 0;
            List<uint> counts = new List<uint>();
            for (int t = 0; t < Threads; t++)
            {
                for (int i = 0; i < FramesPerThread; i++)
                {
                    byte[] plain = Decrypt(reader, frames[t][i]);
                    PacketId id = i % 2 == 0 ? PacketId.S2C_ACHIEVEMENT_ACHIEVEMENT_COMPLETE_NTC : PacketId.S2C_CONNECTION_PING_RES;
                    bool same = plain[0] == id.GroupId
                                && plain[1] == id.HandlerId >> 8
                                && plain[2] == unchecked((byte)id.HandlerId)
                                && plain[3] == id.HandlerSubId
                                && plain[4] == (byte)PacketSource.Server
                                && plain.AsSpan(HeaderSize, BodySize).SequenceEqual(Body(t, i));
                    if (!same)
                    {
                        corrupted++;
                    }
                    else if (id.HandlerSubId == 16)
                    {
                        counts.Add(Count(plain));
                    }
                    else
                    {
                        Assert.Equal(0u, Count(plain));
                    }
                }
            }

            Assert.True(corrupted == 0, $"{corrupted} of {Threads * FramesPerThread} frames did not decrypt to what was written");

            // Each NTC takes its own count, with none lost.
            counts.Sort();
            Assert.Equal(Enumerable.Range(1, Threads * FramesPerThread / 2).Select(count => (uint)count), counts);
        }

        [Fact]
        public void Each_notice_written_on_several_threads_takes_its_own_count()
        {
            // Short frames, so that the threads spend their time near the count and a lost one shows.
            const int notices = 100000;
            byte[][][] frames = new byte[Threads][][];
            Barrier start = new Barrier(Threads);
            PacketFactory factory = NewFactory();
            Thread[] threads = Enumerable.Range(0, Threads).Select(t => new Thread(() =>
            {
                frames[t] = new byte[notices][];
                start.SignalAndWait();
                for (int i = 0; i < notices; i++)
                {
                    frames[t][i] = factory.Write(new Packet(PacketId.S2C_ACHIEVEMENT_ACHIEVEMENT_COMPLETE_NTC, []));
                }
            })).ToArray();

            foreach (Thread thread in threads)
            {
                thread.Start();
            }

            foreach (Thread thread in threads)
            {
                thread.Join();
            }

            PacketFactory reader = NewFactory();
            int distinct = frames.SelectMany(frame => frame).Select(frame => Count(Decrypt(reader, frame))).Distinct().Count();
            Assert.Equal(Threads * notices, distinct);
        }

        [Fact]
        public void Frames_written_on_one_thread_keep_their_bytes()
        {
            PacketFactory factory = NewFactory();
            string[] frames = new[]
                {
                    PacketId.S2C_ACHIEVEMENT_ACHIEVEMENT_COMPLETE_NTC,
                    PacketId.S2C_CONNECTION_PING_RES,
                    PacketId.S2C_ACHIEVEMENT_ACHIEVEMENT_COMPLETE_NTC,
                }
                .Select((id, i) => Convert.ToHexString(factory.Write(new Packet(id, Body(0, i)[..(5 + i * 7)]))))
                .ToArray();

            // Written by the code before overlapping sends were handled.
            Assert.Equal(
                new[]
                {
                    "00109C9593E7E8D2D55DBC4CCD4F6278A263",
                    "00202576F1BFFA005AC85BC447AB3540B7FBC3124AE6AC5CB0DE4B6D2F4CA2A72F0B",
                    "002033E8556DA9233E167B86993958188BADA07C5BC1F3240E0D5A4707173EFAEBB6",
                },
                frames);
        }
    }
}
