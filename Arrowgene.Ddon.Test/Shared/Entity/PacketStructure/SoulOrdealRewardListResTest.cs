using Arrowgene.Ddon.Shared.Entity;
using Arrowgene.Ddon.Shared.Entity.PacketStructure;
using Xunit;

namespace Arrowgene.Ddon.Test.Shared.Entity.PacketStructure
{
    public class SoulOrdealRewardListResTest
    {
        // 62.33.2: levels 1, 4, 1, 4, 1 beside buff ids 10, 18, 1, 42, 66.
        [Fact]
        public void A_captured_response_writes_back_its_levels()
        {
            CapturedFrame.AssertRoundTrip<S2CSeasonDungeonGetSoulOrdealRewardListRes>(CapturedFrame.Body("62_33_2.bin"), 266);
        }
    }
}
