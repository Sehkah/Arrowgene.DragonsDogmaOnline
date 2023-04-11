using System.Collections.Generic;
using Arrowgene.Buffers;

namespace Arrowgene.Ddon.Client.Resource;

/**
 * rJobTutorialQuestList : cResource
 */
public class JobTutorialQuestList : ResourceFile
{
    // MtTypedArray<rJobTutorialQuestList::cQuestId> rJobTutorialQuestList::QuestIdArray;
    public List<uint> JobTutorialList { get; }
    public ushort Version { get; set; }
    public uint Count { get; set; }

    public JobTutorialQuestList()
    {
        JobTutorialList = new List<uint>();
    }

    protected override void ReadResource(IBuffer buffer)
    {
        Version = ReadUInt16(buffer);
        Count = ReadUInt32(buffer);
        JobTutorialList.Clear();
        for (var i = 0; i < Count; i++)
        {
            JobTutorialList.Add(ReadUInt32(buffer));
        }
    }
}
