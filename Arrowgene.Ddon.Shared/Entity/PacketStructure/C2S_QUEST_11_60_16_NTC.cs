using Arrowgene.Buffers;
using Arrowgene.Ddon.Shared.Entity.Structure;
using Arrowgene.Ddon.Shared.Network;

namespace Arrowgene.Ddon.Shared.Entity.PacketStructure
{
    public class C2S_QUEST_11_60_16_NTC : IPacketStructure
    {
        public PacketId Id => PacketId.C2S_QUEST_11_60_16_NTC;

        public C2S_QUEST_11_60_16_NTC()
        {
        }

        public bool IsReturnSession { get; set; } // PC 0x8395a0, from cPRTManager. Name inferred.

        public class Serializer : PacketEntitySerializer<C2S_QUEST_11_60_16_NTC>
        {
            public override void Write(IBuffer buffer, C2S_QUEST_11_60_16_NTC obj)
            {
                WriteBool(buffer, obj.IsReturnSession);
            }

            public override C2S_QUEST_11_60_16_NTC Read(IBuffer buffer)
            {
                C2S_QUEST_11_60_16_NTC obj = new C2S_QUEST_11_60_16_NTC();
                obj.IsReturnSession = ReadBool(buffer);
                return obj;
            }
        }
    }
}
