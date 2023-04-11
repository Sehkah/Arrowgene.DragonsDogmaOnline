using System.Collections.Generic;
using Arrowgene.Buffers;

namespace Arrowgene.Ddon.Client.Resource;

/**
 * rAdjustParam : rTbl2<cAdjustParam> : rTbl2Base : cResource
 */
public class JobAdjustParam : ClientFile
{
    public Tbl2 Table { get; }

    public JobAdjustParam()
    {
        Table = new Tbl2
        {
            Data = new List<AdjustParam>()
        };
    }

    public class Tbl2
    {
        public uint DataVersion { get; set; }
        public uint DataNum { get; set; }
        public List<AdjustParam> Data { get; init; }
    }

    /**
     * cAdjustParam : MtObject
     */
    public class AdjustParam
    {
        public float Param { get; set; }
    }

    protected override void Read(IBuffer buffer)
    {
        Table.DataVersion = buffer.ReadUInt32();
        Table.DataNum = buffer.ReadUInt32();
        for (var i = 0; i < Table.DataNum; i++)
        {
            Table.Data.Add(ReadAdjustParam(buffer));
        }
    }

    private static AdjustParam ReadAdjustParam(IBuffer buffer)
    {
        var data = new AdjustParam
        {
            Param = buffer.ReadFloat()
        };
        return data;
    }
}
