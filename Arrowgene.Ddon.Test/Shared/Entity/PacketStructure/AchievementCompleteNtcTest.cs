using Arrowgene.Ddon.Shared.Entity;
using Arrowgene.Ddon.Shared.Entity.PacketStructure;
using Xunit;

namespace Arrowgene.Ddon.Test.Shared.Entity.PacketStructure
{
    public class AchievementCompleteNtcTest
    {
        // 27.8.16: count 1, id 2532, category 4.
        private const string Capture = "27_8_16.bin";

        [Fact]
        public void A_captured_notice_reads_and_writes_id_and_category()
        {
            CapturedFrame.AssertRoundTrip<S2CAchievementCompleteNtc>(CapturedFrame.Body(Capture), 9);
        }
    }
}
