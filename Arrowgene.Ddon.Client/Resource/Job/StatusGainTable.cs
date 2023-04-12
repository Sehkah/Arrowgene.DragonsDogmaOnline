using System.Collections.Generic;
using Arrowgene.Buffers;

namespace Arrowgene.Ddon.Client.Resource.Job;

/**
 * rStatusGainTable : rTbl2<cStatusGain> : rTbl2Base : cResource
 */
public class StatusGainTable : ClientFile
{
    public Tbl2 Table { get; }

    public StatusGainTable()
    {
        Table = new Tbl2
        {
            Data = new List<StatusGain>()
        };
    }

    public class Tbl2
    {
        public uint DataVersion { get; set; }
        public uint DataNum { get; set; }
        public List<StatusGain> Data { get; init; }
    }

    /**
     * cStatusGain : MtObject
     */
    public class StatusGain
    {
        public uint RequiredDogma { get; set; }
        public uint UpStatusValue { get; set; }
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

    private static StatusGain ReadIncreaseParam2(IBuffer buffer)
    {
        var data = new StatusGain
        {
            RequiredDogma = buffer.ReadUInt32(),
            UpStatusValue = buffer.ReadUInt32(),
        };
        return data;
    }
}
