using Arrowgene.Ddon.Shared.Entity;
using Arrowgene.Ddon.Shared.Entity.PacketStructure;
using Xunit;

namespace Arrowgene.Ddon.Test.Shared.Entity.PacketStructure
{
    public class GetGameSessionKeyResTest
    {
        // 2.1.2: the padding after the u16 is not zero.
        [Fact]
        public void A_captured_response_ends_after_the_u16()
        {
            CapturedFrame.AssertRoundTrip<L2CGetGameSessionKeyRes>(CapturedFrame.Body("2_1_2.bin"), 43);
        }
    }
}
