using Arrowgene.Ddon.Shared.Entity;
using Arrowgene.Ddon.Shared.Entity.PacketStructure;
using Xunit;

namespace Arrowgene.Ddon.Test.Shared.Entity.PacketStructure
{
    public class DroppedListResTest
    {
        // 13.16.2.
        [Fact]
        public void Treasure_point_categories_are_kept()
        {
            CapturedFrame.AssertRoundTrip<S2CInstanceTreasurePointGetCategoryListRes>(CapturedFrame.Body("13_16_2.bin"), 148);
        }

        // 11.4.2.
        [Fact]
        public void Lot_quests_are_kept()
        {
            CapturedFrame.AssertRoundTrip<S2CQuestGetLotQuestListRes>(CapturedFrame.Body("11_4_2.bin"), 16);
        }
    }
}
