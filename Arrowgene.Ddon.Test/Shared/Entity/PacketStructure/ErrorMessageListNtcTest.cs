using Arrowgene.Ddon.Shared.Entity;
using Arrowgene.Ddon.Shared.Entity.PacketStructure;
using Xunit;

namespace Arrowgene.Ddon.Test.Shared.Entity.PacketStructure
{
    public class ErrorMessageListNtcTest
    {
        // 3.0.16: 50 elements each. One is the last of its connection, with 0 to come; the other has 50 to come.
        [Theory]
        [InlineData("3_0_16.last.bin")]
        [InlineData("3_0_16.more_to_come.bin")]
        public void A_captured_notice_reads_and_writes_the_trailing_count(string capture)
        {
            CapturedFrame.AssertRoundTrip<L2CGetErrorMessageListNtc>(CapturedFrame.Body(capture), 2979);
        }
    }
}
