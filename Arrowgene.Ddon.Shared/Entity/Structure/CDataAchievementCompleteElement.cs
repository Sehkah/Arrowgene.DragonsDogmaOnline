using Arrowgene.Buffers;
using Arrowgene.Ddon.Shared.Model;

namespace Arrowgene.Ddon.Shared.Entity.Structure
{
    /// <summary>
    /// An element of 27.8.16. PC 0x758720 reads a u32 and a u8; 0x7c2f80 queues a notice when the u8 is 6.
    /// </summary>
    public class CDataAchievementCompleteElement
    {
        public CDataAchievementCompleteElement()
        {
        }

        public CDataAchievementCompleteElement(uint achievementId, AchievementCategory category)
        {
            AchievementId = achievementId;
            Category = category;
        }

        public uint AchievementId { get; set; }
        public AchievementCategory Category { get; set; }

        public class Serializer : EntitySerializer<CDataAchievementCompleteElement>
        {
            public override void Write(IBuffer buffer, CDataAchievementCompleteElement obj)
            {
                WriteUInt32(buffer, obj.AchievementId);
                WriteByte(buffer, (byte)obj.Category);
            }

            public override CDataAchievementCompleteElement Read(IBuffer buffer)
            {
                CDataAchievementCompleteElement obj = new CDataAchievementCompleteElement();
                obj.AchievementId = ReadUInt32(buffer);
                obj.Category = (AchievementCategory)ReadByte(buffer);
                return obj;
            }
        }
    }
}
