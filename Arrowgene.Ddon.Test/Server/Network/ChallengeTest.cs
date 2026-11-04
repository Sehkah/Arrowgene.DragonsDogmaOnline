using System.Linq;
using System.Security.Cryptography;
using Arrowgene.Ddon.Server.Network;
using Arrowgene.Ddon.Shared.Entity;
using Arrowgene.Ddon.Shared.Entity.PacketStructure;
using Arrowgene.Ddon.Shared.Model;
using Arrowgene.Ddon.Shared.Network;
using Xunit;
using GameChallengeHandler = Arrowgene.Ddon.GameServer.Handler.ClientChallengeHandler;
using LoginChallengeHandler = Arrowgene.Ddon.LoginServer.Handler.ClientChallengeHandler;

namespace Arrowgene.Ddon.Test.Server.Network
{
    public class ChallengeTest
    {
        private const int CertificateSize = 304; // docs: OpenModulo[256], OpenExpo[16], OpenHash[20], 12 of padding.
        private const int HashOffset = 256 + 16;

        private static readonly byte[] SessionKey = Enumerable.Range(0, 32).Select(i => (byte)('a' + i % 26)).ToArray();

        /// <summary>
        /// The whole good path, as a client walks it: the headerless certificate under the fixed key, its hash, the
        /// headerless request, the response, then a normal packet under the new key. Passes on develop too: it pins that
        /// the path stays byte-identical.
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
            C2SCertClientChallengeReq request = clientSide.ClientCreateChallengeRequest(SessionKey);
            PacketFactory clientFactory = new PacketFactory(PacketIdResolver.GamePacketIdResolver);
            byte[] frame = clientFactory.WriteDataWithLengthPrefix(EntitySerializer.Get<C2SCertClientChallengeReq>().Write(request));

            IPacket received = Assert.Single(client.Receive(frame));
            Assert.Equal(PacketId.C2S_CERT_CLIENT_CHALLENGE_REQ, received.Id);
            new GameChallengeHandler(null).Handle(client, received);

            // 9 of header, 8 of RES header, 2 sizes, 64 of password and padding, then padding to the block.
            Assert.Equal(2, client.Frames.Count);
            SentFrame response = client.Frames[1];
            Assert.Equal(96, response.Plain.Length);
            Assert.Equal(PacketId.S2C_CERT_CLIENT_CHALLENGE_RES.GroupId, response.GroupId);
            Assert.Equal(PacketId.S2C_CERT_CLIENT_CHALLENGE_RES.HandlerSubId, response.HandlerSubId);
            Assert.Equal(0u, response.Error);
            Assert.Equal(request.PasswordEnc, response.Plain[19..(19 + request.PasswordEncSize)]);
            Assert.Equal(0, client.Closes);

            clientFactory.SetCamelliaKey(SessionKey);
            IPacket next = Assert.Single(client.Receive(clientFactory.Write(new Packet(PacketId.C2S_CONNECTION_PING_REQ, new byte[4]))));
            Assert.Equal(PacketId.C2S_CONNECTION_PING_REQ, next.Id);
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
        public void A_session_key_of_the_wrong_length_is_a_failed_challenge(int length)
        {
            RecordingGameClient client = new RecordingGameClient(false);

            new GameChallengeHandler(null).Handle(client, GameRequest(client, new byte[length]));

            Assert.Equal((uint)ErrorCode.ERROR_CODE_SYSTEM_INTERNAL, client.Frames[1].Error);
            Assert.Equal(1, client.Closes);
        }
    }
}
