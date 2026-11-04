using Arrowgene.Ddon.Shared.Entity;
using Arrowgene.Ddon.Shared.Entity.PacketStructure;
using Xunit;

namespace Arrowgene.Ddon.Test.Shared.Entity.PacketStructure
{
    public class ClientChallengeTest
    {
        private const int RequestLength = 1 + 256 + 3 + 1 + 1 + 62;
        private const int ResponseLength = 8 + 1 + 1 + 62;

        // The first packet has no header: 1.0.1 and 44.0.1.
        [Fact]
        public void A_captured_login_request_is_read_whole()
        {
            C2LClientChallengeReq read = CapturedFrame.AssertRoundTrip<C2LClientChallengeReq>(CapturedFrame.Raw("1_0_1.bin"), RequestLength);

            Assert.Equal(256, read.CommonKeyEnc.Length);
            Assert.Equal(62, read.PasswordEnc.Length);
            Assert.Equal(16, read.PasswordEncSize);
        }

        [Fact]
        public void A_captured_game_request_is_read_whole()
        {
            C2SCertClientChallengeReq read = CapturedFrame.AssertRoundTrip<C2SCertClientChallengeReq>(CapturedFrame.Raw("44_0_1.bin"), RequestLength);

            Assert.Equal(256, read.CommonKeyEnc.Length);
            Assert.Equal(62, read.PasswordEnc.Length);
            Assert.Equal(16, read.PasswordEncSize);
        }

        // The official server leaves stale bytes after the 16 of the password, so these show the writer keeps all 62.
        // 1.0.2 and 44.0.2.
        [Fact]
        public void A_captured_login_response_is_read_whole()
        {
            CapturedFrame.AssertRoundTrip<L2CClientChallengeRes>(CapturedFrame.Body("1_0_2.bin"), ResponseLength);
        }

        [Fact]
        public void A_captured_game_response_is_read_whole()
        {
            CapturedFrame.AssertRoundTrip<S2CCertClientChallengeRes>(CapturedFrame.Body("44_0_2.bin"), ResponseLength);
        }

        [Fact]
        public void A_zero_in_the_password_does_not_end_the_login_block()
        {
            byte[] request = CapturedFrame.Raw("1_0_1.bin");
            int block = 1 + 256 + 3 + 1 + 1;
            request[block + 3] = 0;

            C2LClientChallengeReq read = EntitySerializer.Get<C2LClientChallengeReq>().Read(request);

            Assert.Equal(62, read.PasswordEnc.Length);
            Assert.Equal(request[(block + 4)..(block + 16)], read.PasswordEnc[4..16]);
        }

        [Fact]
        public void A_request_with_a_short_password_is_written_with_the_whole_block()
        {
            byte[] written = EntitySerializer.Get<C2SCertClientChallengeReq>().Write(new C2SCertClientChallengeReq { PasswordEncSize = 16, PasswordEnc = new byte[16] });

            Assert.Equal(RequestLength, written.Length);
        }
    }
}
