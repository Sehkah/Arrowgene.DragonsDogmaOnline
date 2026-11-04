using System.Linq;
using Arrowgene.Ddon.Shared.Entity;
using Arrowgene.Ddon.Shared.Entity.PacketStructure;
using Arrowgene.Ddon.Shared.Entity.Structure;
using Xunit;

namespace Arrowgene.Ddon.Test.Shared.Entity.PacketStructure
{
    public class CraftProgressTest
    {
        // 30.0.2. No capture holds an element of this list,
        // or of the progress list in any capture with an entry in that list: the element's layout rests on PC 0x75e8e0.
        [Fact]
        public void A_captured_response_still_round_trips()
        {
            CapturedFrame.AssertRoundTrip<S2CCraftGetCraftProgressListRes>(CapturedFrame.Body("30_0_2.bin"), 48);
        }

        [Fact]
        public void An_element_of_the_third_list_is_a_u32()
        {
            EntitySerializer<CDataCraftProgress> serializer = EntitySerializer.Get<CDataCraftProgress>();
            int empty = serializer.Write(new CDataCraftProgress()).Length;

            CDataCraftProgress progress = new CDataCraftProgress();
            progress.CraftMasterLegendPawnInfoList.Add(new CDataCommonU32(7));
            byte[] written = serializer.Write(progress);

            Assert.Equal(empty + 4, written.Length);
            Assert.Equal(7u, serializer.Read(written).CraftMasterLegendPawnInfoList.Single().Value);
        }
    }
}
