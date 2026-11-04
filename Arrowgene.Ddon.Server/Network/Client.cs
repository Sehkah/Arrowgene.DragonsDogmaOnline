using Arrowgene.Ddon.Shared.Entity;
using Arrowgene.Ddon.Shared.Entity.PacketStructure;
using Arrowgene.Ddon.Shared.Network;
using Arrowgene.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Arrowgene.Networking.SAEAServer;

namespace Arrowgene.Ddon.Server.Network
{
    public class Client
    {
        private static uint IncrementingId = 0;

        private readonly ServerLogger Logger;

        protected readonly ClientHandle ClientHandle;
        private readonly PacketFactory _packetFactory;
        private Challenge _challenge;

        /**
         * Roundtrip Challenge completed
         * The client considered to be initialized with crypto key
         * and able to parse packet headers.
         */
        private bool _challengeCompleted;

        /**
         * The received stream could not be followed. The connection has been closed,
         * and anything still on its way in is dropped.
         */
        private bool _receiveFailed;

        public Client(ClientHandle clientHandle, PacketFactory packetFactory) : this(packetFactory, clientHandle.Identity)
        {
            ClientHandle = clientHandle;
        }

        /// <summary>
        /// A client with no socket behind it. A subclass using this overrides <see cref="SendRaw"/> and <see cref="Close"/>.
        /// </summary>
        protected Client(PacketFactory packetFactory, string identity)
        {
            Logger = LogProvider.Logger<ServerLogger>(GetType());
            _packetFactory = packetFactory;
            _challenge = null;
            Identity = identity;
            _challengeCompleted = false;
            Id = IncrementingId++;
        }

        public string Identity { get; protected set; }
        public uint Id { get; protected set; }

        public DateTime PingTime { get; set; }

        public PacketId LastPacketSentToServer { get; set; }
        public PacketId LastPacketSentToClient { get; set; }

        public bool IsAlive
        {
            get
            {
                try
                {
                    // ensure ClientHandle is still valid
                    return ClientHandle.IsAlive;
                }
                catch (ObjectDisposedException)
                {
                    return false;
                }
            }
        }

        ~Client()
        {
            // Something is funny in the event handling for disconnections, so I'm burying this logging here to make sure this absolutely gets called at some point.
            // This may be divorced in the log from the actual event, but you can reconstruct this based on the IP address and the rough timing.
            Logger.Debug(this,
                $"Final Packets: {this?.LastPacketSentToServer.Name ?? "NULL"} / {this?.LastPacketSentToClient.Name ?? "NULL"}");
        }

        public void SetChallengeCompleted(bool challengeCompleted)
        {
            _challengeCompleted = challengeCompleted;
            Logger.Info(this, $"SetChallengeCompleted:{_challengeCompleted}");
        }

        public virtual void Close()
        {
            ClientHandle.Disconnect();
        }

        public List<IPacket> Receive(byte[] data)
        {
            if (_receiveFailed)
            {
                return [];
            }

            List<IPacket> packets;
            try
            {
                packets = _packetFactory.Read(data);
                _receiveFailed = _packetFactory.ReadFailed;
            }
            catch (Exception ex)
            {
                Logger.Exception(this, ex);
                packets = [];
                _receiveFailed = true;
            }

            if (_receiveFailed)
            {
                // PacketFactory could not follow the stream, as with a frame the cipher can not take or a bad source byte.
                // We shouldn't tolerate these connections and just kick them. A client that lost its place is lost anyway.
                Close();
            }

            if (packets.Count > 0)
            {
                LastPacketSentToServer = packets.Last().Id;
            }

            foreach (IPacket packet in packets)
            {
                Logger.LogPacket(this, packet);
            }

            return packets;
        }

        /// <summary>
        /// Send a Structure
        /// </summary>
        public void Send<TResStruct>(TResStruct res) where TResStruct : class, IPacketStructure, new()
        {
            StructurePacket<TResStruct> packet = new StructurePacket<TResStruct>(res);

            Send(packet);
        }

        public void Enqueue<TResStruct>(TResStruct res, PacketQueue queue)
            where TResStruct : class, IPacketStructure, new()
        {
            StructurePacket<TResStruct> packet = new StructurePacket<TResStruct>(res);
            queue.Enqueue((this, packet));
        }

        public void Enqueue(Packet res, PacketQueue queue)
        {
            queue.Enqueue((this, res));
        }

        /// <summary>
        /// Send the one response to a request, and throw if it could not be written, so that the handler can answer
        /// with an error instead: a request with no response ends in a timeout and a disconnect (PC 03040008 0x8855a0).
        /// </summary>
        public void SendResponse<TResStruct>(TResStruct res) where TResStruct : class, IPacketStructure, new()
        {
            if (!TrySend(new StructurePacket<TResStruct>(res)))
            {
                throw new InvalidOperationException($"{typeof(TResStruct).Name} could not be written");
            }
        }

        public void Send(Packet packet)
        {
            TrySend(packet);
        }

        /// <summary>
        /// Send a packet, returning false if it could not be written. A packet held back because the client is not
        /// ready yet was dealt with as intended, so that returns true.
        /// </summary>
        public bool TrySend(Packet packet)
        {
            if (!_challengeCompleted
                && packet.Id != PacketId.S2C_CERT_CLIENT_CHALLENGE_RES
                && packet.Id != PacketId.L2C_CLIENT_CHALLENGE_RES
               )
            {
                // at this point in time we only allow to send S2C_CERT_CLIENT_CHALLENGE_RES
                // only after receiving the first client packet, we can assume the client is able
                // to parse packets headers and process other packets.
                Logger.Debug(this,
                    $"Tried to send Packet:\"{packet.PrintHeader()}\", while client not yet considered ready");
                return true;
            }

            byte[] data;
            try
            {
                data = _packetFactory.Write(packet);
            }
            catch (Exception ex)
            {
                Logger.Exception(this, ex);
                return false;
            }

            if (data == null)
            {
                return false;
            }

            LastPacketSentToClient = packet.Id;

            SendRaw(data);

            try
            {
                Logger.LogPacket(this, packet);
            }
            catch (Exception ex)
            {
                // The packet has left: this must not read as a failed send.
                Logger.Exception(this, ex);
            }

            return true;
        }

        /// <summary>
        /// Sends raw bytes to the client, without any further processing
        /// </summary>
        public virtual void SendRaw(byte[] data)
        {
            ClientHandle.Send(data);
        }

        public void InitializeChallenge()
        {
            _challenge = new Challenge();
            byte[] challenge = _challenge.CreateClientCertChallenge();
            byte[] data;
            try
            {
                data = _packetFactory.WriteDataWithLengthPrefix(challenge);
            }
            catch (Exception ex)
            {
                Logger.Exception(this, ex);
                return;
            }

            SendRaw(data);
        }


        public Challenge.Response HandleChallenge(C2SCertClientChallengeReq request)
        {
            Challenge.Response challenge = _challenge.HandleClientCertChallenge(request);
            _challenge = null;
            if (challenge.Error)
            {
                Logger.Error(this, "Failed CertChallenge");
            }

            _packetFactory.SetCamelliaKey(challenge.CamelliaKey);
            return challenge;
        }

        public Challenge.Response HandleChallenge(C2LClientChallengeReq request)
        {
            Challenge.Response challenge = _challenge.HandleClientCertChallenge(request);
            _challenge = null;
            if (challenge.Error)
            {
                Logger.Error(this, "Failed CertChallenge");
            }

            _packetFactory.SetCamelliaKey(challenge.CamelliaKey);
            return challenge;
        }
    }
}
