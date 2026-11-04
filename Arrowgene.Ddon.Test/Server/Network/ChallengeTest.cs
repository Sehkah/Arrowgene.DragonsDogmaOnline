using System.Linq;
using System.Security.Cryptography;
using Arrowgene.Ddon.Server.Network;
using Arrowgene.Ddon.Shared.Entity;
using Arrowgene.Ddon.Shared.Entity.PacketStructure;
using Arrowgene.Ddon.Shared.Model;
using Arrowgene.Ddon.Shared.Network;
using Arrowgene.Ddon.Test.Shared.Entity.PacketStructure;
using Xunit;
using GameChallengeHandler = Arrowgene.Ddon.GameServer.Handler.ClientChallengeHandler;
using LoginChallengeHandler = Arrowgene.Ddon.LoginServer.Handler.ClientChallengeHandler;

namespace Arrowgene.Ddon.Test.Server.Network
{
    public class ChallengeTest
    {
        private const int CertificateSize = 304; // docs: OpenModulo[256], OpenExpo[16], OpenHash[20], 12 of padding. 125 of 125 captures.
        private const int HashOffset = 256 + 16;
        private const int RequestFrameSize = 336; // 125 of 125 captures.

        // What a real client does: PC 03040008 draws 48 characters and RSA-encrypts them all, and ciphers with the first 32.
        private static readonly byte[] CommonKey = Enumerable.Range(0, Challenge.ClientCommonKeySize).Select(i => (byte)('a' + i % 26)).ToArray();
        private static readonly byte[] SessionKey = CommonKey[..32];

        /// <summary>
        /// The whole good path, as a client walks it: the headerless certificate under the fixed key, its hash, the
        /// headerless request with 48 bytes under RSA, the response, then a normal packet under the first 32 of them.
        /// </summary>
        [Fact]
        public void The_good_path_keeps_its_sizes_and_moves_to_the_session_key()
        {
            RecordingGameClient client = new RecordingGameClient(false);

            client.InitializeChallenge();

            SentFrame certificate = Assert.Single(client.Frames);
            Assert.Equal(CertificateSize, certificate.Plain.Length);
            Assert.Equal(CertificateSize + 2, certificate.Raw.Length);
            Assert.Equal(SHA1.HashData(certificate.Plain[..HashOffset]), certificate.Plain[HashOffset..(HashOffset + 20)]);

            Challenge clientSide = new Challenge();
            clientSide.ClientCompleteChallenge(certificate.Plain);
            C2SCertClientChallengeReq request = clientSide.ClientCreateChallengeRequest(CommonKey);
            Assert.Equal(48, request.CommonKeySrcSize);
            PacketFactory clientFactory = new PacketFactory(PacketIdResolver.GamePacketIdResolver);
            byte[] frame = clientFactory.WriteDataWithLengthPrefix(EntitySerializer.Get<C2SCertClientChallengeReq>().Write(request));
            Assert.Equal(RequestFrameSize + 2, frame.Length);

            IPacket received = Assert.Single(client.Receive(frame));
            Assert.Equal(PacketId.C2S_CERT_CLIENT_CHALLENGE_REQ, received.Id);
            new GameChallengeHandler(null).Handle(client, received);

            // 9 of header, 8 of RES header, 2 sizes, 62 of password and padding, then padding to the block.
            Assert.Equal(2, client.Frames.Count);
            SentFrame response = client.Frames[1];
            Assert.Equal(96, response.Plain.Length);
            Assert.Equal(PacketId.S2C_CERT_CLIENT_CHALLENGE_RES.GroupId, response.GroupId);
            Assert.Equal(PacketId.S2C_CERT_CLIENT_CHALLENGE_RES.HandlerSubId, response.HandlerSubId);
            Assert.Equal(0u, response.Error);
            Assert.Equal(request.PasswordEnc, response.Plain[19..(19 + request.PasswordEncSize)]);
            Assert.Equal(0, client.Closes);

            // The client's side holds only the first 32: the response has to read under them, and so does what it sends next.
            clientFactory.SetCamelliaKey(SessionKey);
            Assert.Equal(response.Plain, clientFactory.Decrypt(response.Raw[2..]));
            IPacket next = Assert.Single(client.Receive(clientFactory.Write(new Packet(PacketId.C2S_CONNECTION_PING_REQ, new byte[4]))));
            Assert.Equal(PacketId.C2S_CONNECTION_PING_REQ, next.Id);
            Assert.Equal(0, client.Closes);
        }

        /// <summary>
        /// The login overload walks the same path: both are checked, as a fault in one would refuse every login or every
        /// move to a game server.
        /// </summary>
        [Fact]
        public void The_login_good_path_takes_a_key_of_48()
        {
            RecordingLoginClient client = new RecordingLoginClient(false);

            new LoginChallengeHandler(null).Handle(client, new StructurePacket<C2LClientChallengeReq>(LoginRequest(client, CommonKey)));

            Assert.Equal(2, client.Frames.Count);
            SentFrame response = client.Frames[1];
            Assert.Equal(96, response.Plain.Length);
            Assert.Equal(PacketId.L2C_CLIENT_CHALLENGE_RES.GroupId, response.GroupId);
            Assert.Equal(0u, response.Error);
            Assert.Equal(0, client.Closes);

            PacketFactory clientFactory = new PacketFactory(PacketIdResolver.LoginPacketIdResolver);
            clientFactory.SetCamelliaKey(SessionKey);
            Assert.Equal(response.Plain, clientFactory.Decrypt(response.Raw[2..]));
        }

        /// <summary>
        /// Both overloads with what a real client sends, from the captures: its sizes, its 62-byte block with the stale
        /// bytes after the password, and 48 bytes under this certificate's RSA key, since the captured block is under the
        /// official key.
        /// </summary>
        [Fact]
        public void A_captured_game_request_passes_under_this_key()
        {
            RecordingGameClient client = new RecordingGameClient(false);
            C2SCertClientChallengeReq captured = EntitySerializer.Get<C2SCertClientChallengeReq>().Read(CapturedFrame.Raw("44_0_1.bin"));
            Assert.Equal(48, captured.CommonKeySrcSize);
            captured.CommonKeyEnc = GameRequest(client, CommonKey).Structure.CommonKeyEnc;

            new GameChallengeHandler(null).Handle(client, new StructurePacket<C2SCertClientChallengeReq>(captured));

            Assert.Equal(0u, client.Frames[1].Error);
            Assert.Equal(captured.PasswordEnc, client.Frames[1].Plain[19..(19 + 62)]);
            Assert.Equal(0, client.Closes);
        }

        [Fact]
        public void A_captured_login_request_passes_under_this_key()
        {
            RecordingLoginClient client = new RecordingLoginClient(false);
            C2LClientChallengeReq captured = EntitySerializer.Get<C2LClientChallengeReq>().Read(CapturedFrame.Raw("1_0_1.bin"));
            Assert.Equal(48, captured.CommonKeySrcSize);
            captured.CommonKeyEnc = LoginRequest(client, CommonKey).CommonKeyEnc;

            new LoginChallengeHandler(null).Handle(client, new StructurePacket<C2LClientChallengeReq>(captured));

            Assert.Equal(0u, client.Frames[1].Error);
            Assert.Equal(captured.PasswordEnc, client.Frames[1].Plain[19..(19 + 62)]);
            Assert.Equal(0, client.Closes);
        }

        private static StructurePacket<C2SCertClientChallengeReq> GameRequest(RecordingGameClient client, byte[] key)
        {
            client.InitializeChallenge();
            Challenge clientSide = new Challenge();
            clientSide.ClientCompleteChallenge(client.Frames[0].Plain);
            C2SCertClientChallengeReq request = clientSide.ClientCreateChallengeRequest(key);
            return new StructurePacket<C2SCertClientChallengeReq>(request);
        }

        private static C2LClientChallengeReq LoginRequest(RecordingLoginClient client, byte[] key)
        {
            client.InitializeChallenge();
            Challenge clientSide = new Challenge();
            clientSide.ClientCompleteChallenge(client.Frames[0].Plain);
            C2SCertClientChallengeReq request = clientSide.ClientCreateChallengeRequest(key);
            return new C2LClientChallengeReq
            {
                CommonKeySrcSize = request.CommonKeySrcSize,
                CommonKeyEnc = request.CommonKeyEnc,
                PasswordSrcSize = request.PasswordSrcSize,
                PasswordEncSize = request.PasswordEncSize,
                PasswordEnc = request.PasswordEnc,
            };
        }

        [Fact]
        public void A_failed_game_challenge_answers_once_and_closes()
        {
            RecordingGameClient client = new RecordingGameClient(false);
            client.InitializeChallenge();
            C2SCertClientChallengeReq request = new C2SCertClientChallengeReq
            {
                CommonKeyEnc = new byte[256], // Not an RSA block for this key.
                PasswordEnc = new byte[16],
            };

            new GameChallengeHandler(null).Handle(client, new StructurePacket<C2SCertClientChallengeReq>(request));

            Assert.Equal(2, client.Frames.Count);
            Assert.Equal((uint)ErrorCode.ERROR_CODE_SYSTEM_INTERNAL, client.Frames[1].Error);
            Assert.Equal(1, client.Closes);
        }

        [Fact]
        public void A_failed_login_challenge_answers_once_and_closes()
        {
            RecordingLoginClient client = new RecordingLoginClient(false);
            client.InitializeChallenge();
            C2LClientChallengeReq request = new C2LClientChallengeReq
            {
                CommonKeyEnc = new byte[256],
                PasswordEnc = new byte[16],
            };

            new LoginChallengeHandler(null).Handle(client, new StructurePacket<C2LClientChallengeReq>(request));

            Assert.Equal(2, client.Frames.Count);
            Assert.Equal((uint)ErrorCode.ERROR_CODE_AUTH_LOGIN_FAILED, client.Frames[1].Error);
            Assert.Equal(1, client.Closes);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(16)]
        [InlineData(31)]
        public void A_session_key_too_short_is_a_failed_game_challenge(int length)
        {
            RecordingGameClient client = new RecordingGameClient(false);

            new GameChallengeHandler(null).Handle(client, GameRequest(client, new byte[length]));

            Assert.Equal((uint)ErrorCode.ERROR_CODE_SYSTEM_INTERNAL, client.Frames[1].Error);
            Assert.Equal(1, client.Closes);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(16)]
        [InlineData(31)]
        public void A_session_key_too_short_is_a_failed_login_challenge(int length)
        {
            RecordingLoginClient client = new RecordingLoginClient(false);

            new LoginChallengeHandler(null).Handle(client, new StructurePacket<C2LClientChallengeReq>(LoginRequest(client, new byte[length])));

            Assert.Equal((uint)ErrorCode.ERROR_CODE_AUTH_LOGIN_FAILED, client.Frames[1].Error);
            Assert.Equal(1, client.Closes);
        }

        [Theory]
        [InlineData(32)]
        [InlineData(48)]
        public void A_key_of_at_least_the_session_size_is_kept_on_both_overloads(int length)
        {
            Challenge server = new Challenge();
            Challenge clientSide = new Challenge();
            clientSide.ClientCompleteChallenge(CertificateOf(server));
            byte[] key = Enumerable.Repeat((byte)'k', length).ToArray();
            C2SCertClientChallengeReq game = clientSide.ClientCreateChallengeRequest(key);
            C2LClientChallengeReq login = new C2LClientChallengeReq
            {
                CommonKeySrcSize = game.CommonKeySrcSize,
                CommonKeyEnc = game.CommonKeyEnc,
                PasswordSrcSize = game.PasswordSrcSize,
                PasswordEncSize = game.PasswordEncSize,
                PasswordEnc = game.PasswordEnc,
            };

            Challenge.Response gameResponse = server.HandleClientCertChallenge(game);
            Challenge.Response loginResponse = server.HandleClientCertChallenge(login);

            Assert.False(gameResponse.Error);
            Assert.False(loginResponse.Error);
            Assert.Equal(key, gameResponse.CamelliaKey);
            Assert.Equal(key, loginResponse.CamelliaKey);
        }

        private static byte[] CertificateOf(Challenge challenge) => challenge.CreateClientCertChallenge();
    }
}
