using System.Collections.Generic;
using Arrowgene.Buffers;

namespace Arrowgene.Ddon.Client.Resource.Job;

/**
 * rJobLevelUpTbl2 : rTbl2<cIncreaseParam2> : rTbl2Base : cResource
 */
public class JobLevelUpTbl2 : ClientFile
{
    public Tbl2 Table { get; }

    public JobLevelUpTbl2()
    {
        Table = new Tbl2
        {
            Data = new List<IncreaseParam2>()
        };
    }

    public class Tbl2
    {
        public uint DataVersion { get; set; }
        public uint DataNum { get; set; }
        public List<IncreaseParam2> Data { get; init; }
    }

    /**
     * cIncreaseParam2 : MtObject
     */
    public class IncreaseParam2
    {
        public uint Lv { get; set; }
        public uint Atk { get; set; }
        public uint Def { get; set; }
        public uint MAtk { get; set; }
        public uint MDef { get; set; }
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

    private static IncreaseParam2 ReadIncreaseParam2(IBuffer buffer)
    {
        var data = new IncreaseParam2
        {
            Lv = buffer.ReadUInt32(),
            Atk = buffer.ReadUInt32(),
            Def = buffer.ReadUInt32(),
            MAtk = buffer.ReadUInt32(),
            MDef = buffer.ReadUInt32()
        };
        return data;
    }
}
