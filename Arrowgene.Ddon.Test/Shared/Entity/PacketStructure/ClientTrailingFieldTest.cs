using Arrowgene.Ddon.Shared.Entity;
using Arrowgene.Ddon.Shared.Entity.PacketStructure;
using Xunit;

namespace Arrowgene.Ddon.Test.Shared.Entity.PacketStructure
{
    public class ClientTrailingFieldTest
    {
        // 14.11.16.
        [Fact]
        public void Warp_end_reads_its_u32()
        {
            CapturedFrame.AssertRoundTrip<C2SWarpWarpEndNtc>(CapturedFrame.Body("14_11_16.bin"), 4);
        }

        // 0.1.1.
        [Fact]
        public void Connection_login_reads_the_client_version()
        {
            C2SConnectionLoginReq read = CapturedFrame.AssertRoundTrip<C2SConnectionLoginReq>(CapturedFrame.Body("0_1_1.bin"), 38);

            Assert.Equal(0x3040001Bu, read.ClientVersion);
        }

        // 0.5.1.
        [Fact]
        public void Move_in_server_reads_platform_and_client_version()
        {
            CapturedFrame.AssertRoundTrip<C2SConnectionMoveInServerReq>(CapturedFrame.Body("0_5_1.bin"), 38);
        }

        // 50.1.1.
        [Fact]
        public void Item_sort_reads_its_trailing_bool()
        {
            CapturedFrame.AssertRoundTrip<C2SItemSortSetItemSortDataBinReq>(CapturedFrame.Body("50_1_1.bin"), 1026);
        }

        // 11.60.16.
        [Fact]
        public void Return_session_reads_its_bool()
        {
            CapturedFrame.AssertRoundTrip<C2S_QUEST_11_60_16_NTC>(CapturedFrame.Body("11_60_16.bin"), 1);
        }
    }
}
