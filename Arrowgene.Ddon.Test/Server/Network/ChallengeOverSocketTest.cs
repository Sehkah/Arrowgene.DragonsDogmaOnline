using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Arrowgene.Ddon.LoginServer;
using Arrowgene.Ddon.Server.Network;
using Arrowgene.Ddon.Shared.Entity;
using Arrowgene.Ddon.Shared.Entity.PacketStructure;
using Arrowgene.Ddon.Shared.Model;
using Arrowgene.Ddon.Shared.Network;
using Arrowgene.Networking.SAEAServer;
using Arrowgene.Networking.SAEAServer.Consumer;
using Xunit;
using LoginChallengeHandler = Arrowgene.Ddon.LoginServer.Handler.ClientChallengeHandler;

namespace Arrowgene.Ddon.Test.Server.Network
{
    /// <summary>
    /// The challenge over a real socket and the real send path, as the recording clients have neither.
    /// </summary>
    public class ChallengeOverSocketTest : IDisposable
    {
        private const int TimeoutMs = 10000;

        private readonly TcpServer _server;
        private readonly ushort _port;

        public ChallengeOverSocketTest()
        {
            TcpListener probe = new TcpListener(IPAddress.Loopback, 0);
            probe.Start();
            _port = (ushort)((IPEndPoint)probe.LocalEndpoint).Port;
            probe.Stop();

            TcpServerSettings settings = new TcpServerSettings { Identity = "ChallengeOverSocketTest" };
            _server = new TcpServer(IPAddress.Loopback, _port, new LoginConsumer(), settings);
            _server.Start();
        }

        public void Dispose() => _server.Dispose();

        /// <summary>The handler the login server runs, on the client the login server builds.</summary>
        private class LoginConsumer : IConsumer
        {
            private readonly ConcurrentDictionary<ClientHandle, LoginClient> _clients = new();

            public void OnClientConnected(ClientHandle clientHandle)
            {
                LoginClient client = new LoginClient(clientHandle, new PacketFactory(PacketIdResolver.LoginPacketIdResolver));
                _clients[clientHandle] = client;
                client.InitializeChallenge();
            }

            public void OnReceivedData(ClientHandle clientHandle, byte[] data)
            {
                LoginClient client = _clients[clientHandle];
                foreach (IPacket packet in client.Receive(data))
                {
                    if (packet.Id == PacketId.C2L_CLIENT_CHALLENGE_REQ)
                    {
                        new LoginChallengeHandler(null).Handle(client, packet);
                    }
                }
            }

            public void OnClientDisconnected(ClientSnapshot clientSnapshot)
            {
            }

            public void OnError(ClientSnapshot clientSnapshot, Exception exception, string message)
            {
            }
        }

        private static byte[] ReadExactly(NetworkStream stream, int count)
        {
            byte[] data = new byte[count];
            stream.ReadExactly(data);
            return data;
        }

        private static byte[] ReadFrame(NetworkStream stream)
        {
            byte[] prefix = ReadExactly(stream, 2);
            return ReadExactly(stream, (prefix[0] << 8) | prefix[1]);
        }

        private static uint ErrorOf(byte[] plain) => ((uint)plain[9] << 24) | ((uint)plain[10] << 16) | ((uint)plain[11] << 8) | plain[12];

        private TcpClient Connect()
        {
            TcpClient socket = new TcpClient(AddressFamily.InterNetwork) { ReceiveTimeout = TimeoutMs, SendTimeout = TimeoutMs };
            socket.Connect(IPAddress.Loopback, _port);
            return socket;
        }

        private static byte[] Request(byte[] certificatePlain, byte[] commonKey, PacketFactory factory)
        {
            Challenge clientSide = new Challenge();
            clientSide.ClientCompleteChallenge(certificatePlain);
            C2SCertClientChallengeReq request = clientSide.ClientCreateChallengeRequest(commonKey);
            return factory.WriteDataWithLengthPrefix(EntitySerializer.Get<C2SCertClientChallengeReq>().Write(request));
        }

        [Fact]
        public void A_failed_challenge_delivers_its_response_before_the_close()
        {
            for (int i = 0; i < 25; i++)
            {
                using TcpClient socket = Connect();
                NetworkStream stream = socket.GetStream();
                PacketFactory factory = new PacketFactory(PacketIdResolver.LoginPacketIdResolver);
                byte[] certificate = factory.Decrypt(ReadFrame(stream));

                stream.Write(Request(certificate, new byte[16], factory));

                // The session key was never set, so the response is under the fixed key.
                byte[] response = factory.Decrypt(ReadFrame(stream));
                Assert.Equal(PacketId.L2C_CLIENT_CHALLENGE_RES.GroupId, response[0]);
                Assert.Equal((uint)ErrorCode.ERROR_CODE_AUTH_LOGIN_FAILED, ErrorOf(response));
                Assert.Equal(0, stream.Read(new byte[1])); // An orderly end, not a reset or a timeout.
            }
        }

        [Fact]
        public void A_key_of_48_stays_connected_and_its_response_reads_under_the_first_32()
        {
            using TcpClient socket = Connect();
            NetworkStream stream = socket.GetStream();
            PacketFactory factory = new PacketFactory(PacketIdResolver.LoginPacketIdResolver);
            byte[] certificate = factory.Decrypt(ReadFrame(stream));
            byte[] commonKey = Enumerable.Range(0, Challenge.ClientCommonKeySize).Select(i => (byte)('A' + i % 26)).ToArray();

            stream.Write(Request(certificate, commonKey, factory));

            factory.SetCamelliaKey(commonKey[..32]);
            byte[] response = factory.Decrypt(ReadFrame(stream));
            Assert.Equal(PacketId.L2C_CLIENT_CHALLENGE_RES.GroupId, response[0]);
            Assert.Equal(0u, ErrorOf(response));

            socket.ReceiveTimeout = 200;
            IOException quiet = Assert.Throws<IOException>(() => stream.Read(new byte[1])); // Nothing more, and no close.
            Assert.Equal(SocketError.TimedOut, ((SocketException)quiet.InnerException).SocketErrorCode);
        }
    }
}
