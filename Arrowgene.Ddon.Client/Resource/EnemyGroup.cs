using System.Collections.Generic;
using Arrowgene.Buffers;

namespace Arrowgene.Ddon.Client.Resource
{
    public class EnemyGroup : ClientFile
    {
        public class Entry
        {
            public uint mEnemyGroupId { get; set; }
            public uint mMsgIndex { get; set; }
            public List<uint> mEmList { get; set; }

            public Entry()
            {
                mEmList = new List<uint>();
            }
        }

        public List<Entry> Entries { get; }

        public EnemyGroup()
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
            entry.mEnemyGroupId = ReadUInt32(buffer);
            entry.mMsgIndex = ReadUInt32(buffer);
            entry.mEmList = ReadMtArray(buffer, ReadEmId);
            return entry;
        }

        private uint ReadEmId(IBuffer buffer)
        {
            return ReadUInt32(buffer);
        }
    }
}
