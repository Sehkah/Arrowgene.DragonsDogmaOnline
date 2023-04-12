using System.Collections.Generic;
using Arrowgene.Buffers;

namespace Arrowgene.Ddon.Client.Resource
{
    
    
    public class StageList : ResourceFile
    {
        
        enum STAGE_TYPE
        {
            STAGE_TYPE_NONE = 0x0,
            STAGE_TYPE_LOBBY = 0x1,
            STAGE_TYPE_FIELD = 0x2,
            STAGE_TYPE_SAFE_AREA = 0x3,
            STAGE_TYPE_DUNGEON = 0x4,
        };

        enum STAGE_NUMBER
        {
            STAGE_FIELD = 0x64,
            STAGE_MOVIE = 0x96,
            STAGE_ENDING = 0x97,
            STAGE_CHAR_EDIT = 0xA0,
            STAGE_LOBBY = 0xC8,
            STAGE_LOBBY_TEMPLE = 0xC9,
            STAGE_LOBBY_HARBOR = 0xCB,
            STAGE_CLAN_BASE = 0xD2,
            STAGE_MYROOM = 0xD3,
            STAGE_SKILL_CHECK = 0xD4
        };
        
        // rStageList::Info vftable:0x1C6EF0C, Size:0x18, CRC32:0x43B85BE0
        public class Info
        {
            public uint StageNo { get; set; }
            public uint Type { get; set; }
            // Custom attribute for user friendly name resolution
            public string TypeName { get; set; }
            public byte RecommendLevel { get; set; }
            public uint MessageId { get; set; }
            public uint Version { get; set; }
        }

        public List<Info> StageInfos { get; }

        public StageList()
        {
            StageInfos = new List<Info>();
        }
        
        protected override void ReadResource(IBuffer buffer)
        {
            uint version = ReadUInt32(buffer);
            StageInfos.Clear();
            List<Info> infos = ReadMtArray(buffer, ReadEntry);
            StageInfos.AddRange(infos);
        }

        private Info ReadEntry(IBuffer buffer)
        {
            Info entry = new Info();
            entry.StageNo = ReadUInt32(buffer);
            entry.Type = ReadUInt32(buffer);
            entry.TypeName = ((STAGE_TYPE)entry.Type).ToString();
            entry.RecommendLevel = ReadByte(buffer);
            entry.MessageId = ReadUInt32(buffer);
            entry.Version = ReadUInt32(buffer);
            return entry;
        }
    }
}
