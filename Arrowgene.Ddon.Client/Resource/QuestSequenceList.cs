using System.Collections.Generic;
using Arrowgene.Buffers;

namespace Arrowgene.Ddon.Client.Resource;

/**
 * rQuestSequenceList : rTbl2<cQuestSequence> : rTbl2Base : cResource
 */
public class QuestSequenceList : ClientFile
{
    public Tbl2 Table { get; }

    public QuestSequenceList()
    {
        Table = new Tbl2
        {
            Data = new List<QuestSequence>()
        };
    }

    public class Tbl2
    {
        public uint DataVersion { get; set; }
        public uint DataNum { get; set; }
        public List<QuestSequence> Data { get; init; }
    }

    /**
     * cStatusGain : MtObject
     */
    public class QuestSequence
    {
        public uint SeqNo { get; set; }
        public uint QstId { get; set; }
    }

    protected override void Read(IBuffer buffer)
    {
        Table.DataVersion = buffer.ReadUInt32();
        Table.DataNum = buffer.ReadUInt32();
        for (var i = 0; i < Table.DataNum; i++)
        {
            Table.Data.Add(ReadIncreaseParam2(buffer));
        }
    }

    protected override void Write(IBuffer buffer)
    {
        throw new System.NotImplementedException();
    }

    private static QuestSequence ReadIncreaseParam2(IBuffer buffer)
    {
        var data = new QuestSequence
        {
            SeqNo = buffer.ReadUInt32(),
            QstId = buffer.ReadUInt32(),
        };
        return data;
    }
}
