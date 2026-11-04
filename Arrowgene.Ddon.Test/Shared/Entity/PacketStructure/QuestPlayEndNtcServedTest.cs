using System;
using Arrowgene.Ddon.GameServer.Handler;
using Arrowgene.Ddon.Shared.Entity;
using Arrowgene.Ddon.Shared.Entity.PacketStructure;
using Xunit;

namespace Arrowgene.Ddon.Test.Shared.Entity.PacketStructure
{
    public class QuestPlayEndNtcServedTest
    {
        [Fact]
        public void A_served_play_time_is_the_whole_time_in_the_seventh_u32()
        {
            S2CQuestPlayEndNtc served = new S2CQuestPlayEndNtc();
            served.ContentsPlayEnd.PlayTimeMillSec = QuestPlayEndHandler.PlayTimeMilliseconds(TimeSpan.FromMilliseconds(1045461));

            byte[] written = EntitySerializer.Get<S2CQuestPlayEndNtc>().Write(served);

            // A u32 of the notice, then the structure: the 7th u32 of the structure is at 4 + 6 * 4.
            byte[] captured = CapturedFrame.Body("11_45_16.bin");
            Assert.Equal(new byte[] { 0, 0x0F, 0xF3, 0xD5 }, captured[28..32]);
            Assert.Equal(captured[28..32], written[28..32]);
        }

        [Fact]
        public void A_time_a_u32_can_not_hold_is_clamped()
        {
            Assert.Equal(uint.MaxValue, QuestPlayEndHandler.PlayTimeMilliseconds(TimeSpan.FromDays(100)));
            Assert.Equal(0u, QuestPlayEndHandler.PlayTimeMilliseconds(TimeSpan.FromSeconds(-1)));
        }
    }
}
