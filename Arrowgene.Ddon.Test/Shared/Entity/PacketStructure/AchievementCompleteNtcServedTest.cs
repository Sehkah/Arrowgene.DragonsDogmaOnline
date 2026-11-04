using Arrowgene.Ddon.Shared.Entity;
using Arrowgene.Ddon.Shared.Entity.PacketStructure;
using Arrowgene.Ddon.Shared.Entity.Structure;
using Arrowgene.Ddon.Shared.Model;
using Xunit;

namespace Arrowgene.Ddon.Test.Shared.Entity.PacketStructure
{
    public class AchievementCompleteNtcServedTest
    {
        [Fact]
        public void A_served_notice_equals_the_captured_bytes()
        {
            S2CAchievementCompleteNtc served = new S2CAchievementCompleteNtc();
            served.AchievementIdList.Add(new CDataAchievementCompleteElement(2532, AchievementCategory.Collecting));

            byte[] captured = CapturedFrame.Body("27_8_16.bin");
            Assert.Equal(captured[..9], EntitySerializer.Get<S2CAchievementCompleteNtc>().Write(served));
        }

        [Fact]
        public void Two_elements_keep_their_places()
        {
            S2CAchievementCompleteNtc served = new S2CAchievementCompleteNtc();
            served.AchievementIdList.Add(new CDataAchievementCompleteElement(1, AchievementCategory.RoyalFamilyRestoration));
            served.AchievementIdList.Add(new CDataAchievementCompleteElement(2, AchievementCategory.Adventure));

            Assert.Equal(new byte[] { 0, 0, 0, 2, 0, 0, 0, 1, 6, 0, 0, 0, 2, 1 }, EntitySerializer.Get<S2CAchievementCompleteNtc>().Write(served));
        }
    }
}
