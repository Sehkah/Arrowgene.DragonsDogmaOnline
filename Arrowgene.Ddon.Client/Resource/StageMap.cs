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
            public uint mStageNo { get; set; }
            public uint mPartsNum { get; set; }
            public float mOffsetY { get; set; }
            public uint mStageFlag { get; set; }
            public List<cParam> mParamList { get; set; }
            public Entry()
            {
                mParamList = new List<cParam>();
            }
        }

        public class cParam
        {
            public uint mAreaNo { get; set; }
            public float mSize { get; set; }
            public string mModelName { get; set; }
            public MtVector3 mConnectPos { get; set; }
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

        private Entry ReadEntry(IBuffer buffer)
        {
            Entry entry = new Entry();
            entry.mStageNo = ReadUInt16(buffer);
            entry.mPartsNum = ReadUInt16(buffer);
            entry.mOffsetY = ReadFloat(buffer);
            entry.mStageFlag = ReadUInt32(buffer);
            entry.mParamList = ReadMtArray(buffer, ReadCParam);
            return entry;
        }

        private cParam ReadCParam(IBuffer buffer)
        {
            cParam cParam = new cParam();
            cParam.mAreaNo = ReadUInt32(buffer);
            cParam.mSize = ReadFloat(buffer);
            cParam.mModelName = buffer.ReadCString(Encoding.UTF8);
            cParam.mConnectPos = ReadMtVector3(buffer);
            return cParam;
        }
    }
}
