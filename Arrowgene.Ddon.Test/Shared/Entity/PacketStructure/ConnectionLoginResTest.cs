using Arrowgene.Ddon.Shared.Entity;
using Arrowgene.Ddon.Shared.Entity.PacketStructure;
using Xunit;

namespace Arrowgene.Ddon.Test.Shared.Entity.PacketStructure
{
    public class ConnectionLoginResTest
    {
        // 0.1.2: Error, Result, bool 1, a 31-byte token.
        private const string Capture = "0_1_2.bin";
        private const int Length = 8 + 1 + 2 + 31;

        [Fact]
        public void A_captured_response_reads_flag_and_token()
        {
            S2CConnectionLoginRes read = CapturedFrame.AssertRoundTrip<S2CConnectionLoginRes>(CapturedFrame.Body(Capture), Length);

            Assert.True(read.IsCogLogin);
            Assert.Equal(31, read.OneTimeToken.Length);
        }

        [Fact]
        public void A_served_response_has_the_shape_of_the_capture()
        {
            byte[] captured = CapturedFrame.Body(Capture);
            string token = System.Text.Encoding.UTF8.GetString(captured, 11, 31);

            byte[] served = EntitySerializer.Get<S2CConnectionLoginRes>().Write(new S2CConnectionLoginRes { OneTimeToken = token });

            // The server's flag stays false, which the client ignores; every other byte is the capture's.
            Assert.Equal(0, served[8]);
            captured[8] = 0;
            Assert.Equal(captured[..Length], served);
        }
    }
}
