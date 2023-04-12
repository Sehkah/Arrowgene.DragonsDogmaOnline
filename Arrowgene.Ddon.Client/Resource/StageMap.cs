using System.Collections.Generic;
using System.Text;
using Arrowgene.Buffers;
using Arrowgene.Ddon.Shared.Model;

namespace Arrowgene.Ddon.Client.Resource
{
    public class StageMap : ClientFile
    {
        public class Entry
        {
            public uint StageNo { get; set; }
            public uint PartsNum { get; set; }
            public float OffsetY { get; set; }
            public uint StageFlag { get; set; }
            public List<Param> ParamList { get; set; }

            public Entry()
            {
                ParamList = new List<Param>();
            }
        }

        public class Param
        {
            public uint AreaNo { get; set; }
            public float Size { get; set; }
            public string ModelName { get; set; }
            public MtVector3 ConnectPos { get; set; }
        }

        enum STAGE_FLAG
        {
            STAGE_FLAG_DUMMY = 0x1,
            STAGE_FLAG_JOINT = 0x2,
            STAGE_FLAG_RAND = 0x4,
            STAGE_FLAG_CUSTOM = 0x8,
            STAGE_FLAG_LARGE_PARTY = 0x10,
            STAGE_FLAG_WOFFSET = 0x20,
            STAGE_FLAG_REVIVAL_PAWN = 0x40,
            STAGE_FLAG_SOLO = 0x80,
            STAGE_FLAG_CRAFT = 0x100,
            STAGE_FLAG_PAWN_DUGEON = 0x200,
            STAGE_FLAG_WIND_OFF = 0x400,
            STAGE_FLAG_DARK = 0x800,
            STAGE_FLAG_ENVMAP_SKY = 0x1000,
            STAGE_FLAG_MERGODA = 0x2000,
            STAGE_FLAG_MY_ROOM = 0x4000,
            STAGE_FLAG_PARTY_ONLY = 0x8000,
            STAGE_FLAG_DISABLE_CREATE_CHAR = 0x10000000,
            STAGE_FLAG_DISABLE_FADE_IN = 0x20000000,
        };

        public List<Entry> Entries { get; }

        public StageMap()
        {
            Entries = new List<Entry>();
        }

        protected override void Read(IBuffer buffer)
        {
            uint data = buffer.ReadUInt32();
            uint dataNum = buffer.ReadUInt32();
            Entries.Clear();
            for (int i = 0; i < dataNum; i++)
            {
                Entries.Add(ReadEntry(buffer));
            }
        }

        protected override void Write(IBuffer buffer)
        {
            throw new System.NotImplementedException();
        }

        private Entry ReadEntry(IBuffer buffer)
        {
            Entry entry = new Entry();
            entry.StageNo = ReadUInt16(buffer);
            entry.PartsNum = ReadUInt16(buffer);
            entry.OffsetY = ReadFloat(buffer);
            entry.StageFlag = ReadUInt32(buffer);
            entry.ParamList = ReadMtArray(buffer, ReadParam);
            return entry;
        }

        private Param ReadParam(IBuffer buffer)
        {
            Param Param = new Param();
            Param.AreaNo = ReadUInt32(buffer);
            Param.Size = ReadFloat(buffer);
            Param.ModelName = buffer.ReadCString(Encoding.UTF8);
            Param.ConnectPos = ReadMtVector3(buffer);
            return Param;
        }
    }
}
