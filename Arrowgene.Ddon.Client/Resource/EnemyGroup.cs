using System.Collections.Generic;
using Arrowgene.Buffers;

namespace Arrowgene.Ddon.Client.Resource
{
    public class EnemyGroup : ClientFile
    {
        public class Entry
        {
            public uint EnemyGroupId { get; set; }
            public uint MsgIndex { get; set; }
            public List<uint> EmList { get; set; }

            public Entry()
            {
                EmList = new List<uint>();
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

        protected override void Write(IBuffer buffer)
        {
            throw new System.NotImplementedException();
        }

        private Entry ReadEntry(IBuffer buffer)
        {
            Entry entry = new Entry();
            entry.EnemyGroupId = ReadUInt32(buffer);
            entry.MsgIndex = ReadUInt32(buffer);
            entry.EmList = ReadMtArray(buffer, ReadEmId);
            return entry;
        }

        private uint ReadEmId(IBuffer buffer)
        {
            return ReadUInt32(buffer);
        }
    }
}
