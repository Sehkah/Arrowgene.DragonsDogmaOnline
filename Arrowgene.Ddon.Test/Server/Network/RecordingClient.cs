using System;
using System.Collections.Generic;
using NetworkClient = Arrowgene.Ddon.Server.Network.Client;
using Arrowgene.Ddon.GameServer;
using Arrowgene.Ddon.LoginServer;
using Arrowgene.Ddon.Server.Network;

namespace Arrowgene.Ddon.Test.Server.Network
{
    /// <summary>
    /// A frame as it left the server, decrypted under the sending factory's key.
    /// </summary>
    public class SentFrame
    {
        public byte[] Raw { get; init; }
        public byte[] Plain { get; init; }
        public byte GroupId => Plain[0];
        public ushort HandlerId => (ushort)((Plain[1] << 8) | Plain[2]);
        public byte HandlerSubId => Plain[3];

        /// <summary>The first u32 of a RES body.</summary>
        public uint Error => ((uint)Plain[9] << 24) | ((uint)Plain[10] << 16) | ((uint)Plain[11] << 8) | Plain[12];

        public static SentFrame From(PacketFactory factory, byte[] raw)
        {
            byte[] encrypted = new byte[raw.Length - 2];
            Array.Copy(raw, 2, encrypted, 0, encrypted.Length);
            return new SentFrame { Raw = raw, Plain = factory.Decrypt(encrypted) };
        }
    }

    public interface IRecordingClient
    {
        List<SentFrame> Frames { get; }
        int Closes { get; }
    }

    public class RecordingClient : NetworkClient, IRecordingClient
    {
        private readonly PacketFactory _factory;

        public RecordingClient() : this(new PacketFactory(PacketIdResolver.GamePacketIdResolver))
        {
        }

        public RecordingClient(PacketFactory factory) : base(factory, "[Test]")
        {
            _factory = factory;
            SetChallengeCompleted(true);
        }

        public List<SentFrame> Frames { get; } = new List<SentFrame>();
        public int Closes { get; private set; }

        public override void SendRaw(byte[] data) => Frames.Add(SentFrame.From(_factory, data));
        public override void Close() => Closes++;
    }

    public class RecordingGameClient : GameClient, IRecordingClient
    {
        private readonly PacketFactory _factory;

        public RecordingGameClient(bool challengeCompleted = true) : this(new PacketFactory(PacketIdResolver.GamePacketIdResolver), challengeCompleted)
        {
        }

        private RecordingGameClient(PacketFactory factory, bool challengeCompleted) : base(factory, "[TestGame]")
        {
            _factory = factory;
            if (challengeCompleted)
            {
                SetChallengeCompleted(true);
            }
        }

        public List<SentFrame> Frames { get; } = new List<SentFrame>();
        public int Closes { get; private set; }

        public PacketFactory Factory => _factory;

        public override void SendRaw(byte[] data) => Frames.Add(SentFrame.From(_factory, data));
        public override void Close() => Closes++;
    }

    public class RecordingLoginClient : LoginClient, IRecordingClient
    {
        private readonly PacketFactory _factory;

        public RecordingLoginClient(bool challengeCompleted = true) : this(new PacketFactory(PacketIdResolver.LoginPacketIdResolver), challengeCompleted)
        {
        }

        private RecordingLoginClient(PacketFactory factory, bool challengeCompleted) : base(factory, "[TestLogin]")
        {
            _factory = factory;
            if (challengeCompleted)
            {
                SetChallengeCompleted(true);
            }
        }

        public List<SentFrame> Frames { get; } = new List<SentFrame>();
        public int Closes { get; private set; }

        public PacketFactory Factory => _factory;

        public override void SendRaw(byte[] data) => Frames.Add(SentFrame.From(_factory, data));
        public override void Close() => Closes++;
    }
}
