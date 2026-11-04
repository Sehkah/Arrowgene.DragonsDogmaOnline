using Arrowgene.Ddon.Shared.Entity;
using Arrowgene.Ddon.Shared.Entity.PacketStructure;
using Xunit;

namespace Arrowgene.Ddon.Test.Shared.Entity.PacketStructure
{
    public class ErrorMessageListNtcTest
    {
        // 3.0.16: 50 elements, then 50 still to come.
        [Fact]
        public void A_captured_notice_reads_and_writes_the_trailing_count()
        {
            byte[] body = CapturedFrame.Body("3_0_16.bin");

            CapturedFrame.AssertRoundTrip<L2CGetErrorMessageListNtc>(body, 2979);
            Assert.Equal(new byte[] { 0, 0, 0, 50 }, body[2975..2979]);
        }
    }
}
