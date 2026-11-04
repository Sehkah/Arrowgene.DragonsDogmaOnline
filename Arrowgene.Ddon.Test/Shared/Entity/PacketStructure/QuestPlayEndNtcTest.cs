using Arrowgene.Ddon.Shared.Entity;
using Arrowgene.Ddon.Shared.Entity.PacketStructure;
using Xunit;

namespace Arrowgene.Ddon.Test.Shared.Entity.PacketStructure
{
    public class QuestPlayEndNtcTest
    {
        // 11.45.16, the only capture. PC 0xbd4b56 keeps the 7th u32.
        [Fact]
        public void The_play_time_is_the_seventh_u32()
        {
            S2CQuestPlayEndNtc read = CapturedFrame.AssertRoundTrip<S2CQuestPlayEndNtc>(CapturedFrame.Body("11_45_16.bin"), 43);

            Assert.Equal(1045461u, read.ContentsPlayEnd.PlayTimeMillSec);
        }
    }
}
