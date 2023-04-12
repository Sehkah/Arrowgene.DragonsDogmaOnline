using System.Collections.Generic;
using Arrowgene.Buffers;

namespace Arrowgene.Ddon.Client.Resource.Quest;

/**
 * rTutorialQuestGroup : cResource
 */
public class TutorialQuestGroup : ResourceFile
{
    // rTutorialQuestGroup::cGroup : MtObject
    public List<Group> Groups { get; }

    // MtTypedArray<rTutorialQuestGroup::cGroup::cQuestId> rTutorialQuestGroup::cGroup::QuestIdArray;
    public class Group
    {
        public uint GroupId { get; set; }
        public List<uint> QuestIds { get; set; }
    }

    public ushort Version { get; set; }
    public uint Count { get; set; }

    public TutorialQuestGroup()
    {
        Groups = new List<Group>();
    }

    protected override void ReadResource(IBuffer buffer)
    {
        Version = ReadUInt16(buffer);
        Count = ReadUInt32(buffer);
        Groups.Clear();
        for (var i = 0; i < Count; i++)
        {
            Groups.Add(ReadGroup(buffer));
        }
    }

    private Group ReadGroup(IBuffer buffer)
    {
        Group group = new Group();
        group.GroupId = ReadUInt32(buffer);
        uint len = ReadUInt32(buffer);
        group.QuestIds = new List<uint>((int)len);
        group.QuestIds.Clear();
        for (var i = 0; i < len; i++)
        {
            group.QuestIds.Add(ReadUInt32(buffer));
        }

        return group;
    }
}
