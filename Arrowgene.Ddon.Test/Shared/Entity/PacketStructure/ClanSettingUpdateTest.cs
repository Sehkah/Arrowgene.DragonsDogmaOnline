using Arrowgene.Ddon.Shared.Entity;
using Arrowgene.Ddon.Shared.Entity.PacketStructure;
using Xunit;

namespace Arrowgene.Ddon.Test.Shared.Entity.PacketStructure
{
    public class ClanSettingUpdateTest
    {
        // 31.17.2: the body is the RES header alone.
        [Fact]
        public void A_captured_response_is_its_header()
        {
            CapturedFrame.AssertRoundTrip<S2CClanClanSettingUpdateRes>(CapturedFrame.Body("31_17_2.bin"), 8);
        }

        // 31.17.1.
        [Fact]
        public void A_captured_request_is_its_bool()
        {
            CapturedFrame.AssertRoundTrip<C2SClanClanSettingUpdateReq>(CapturedFrame.Body("31_17_1.bin"), 1);
        }
    }
}
