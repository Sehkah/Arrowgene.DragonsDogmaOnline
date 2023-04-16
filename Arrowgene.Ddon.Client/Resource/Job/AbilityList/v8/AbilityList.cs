using System.Collections.Generic;
using Arrowgene.Buffers;

namespace Arrowgene.Ddon.Client.Resource.Job.AbilityList.v8;

public class AbilityList : ResourceFile
{
    public uint DataListLength { get; set; }
    public List<AbilityData> DataList { get; set; }

    // TODO Unknown additional data needs to be understood, but otherwise the basic parsing logic works
    protected override void ReadResource(IBuffer buffer)
    {
        ReadHeader(buffer);
        DataList = new List<AbilityData>((int)DataListLength);
        DataList.Clear();
        ReadData(buffer);
    }

    public virtual void ReadHeader(IBuffer buffer)
    {
        var UnknownByte1 = ReadByte(buffer);
        var UnknownByte2 = ReadByte(buffer);
        var UnknownShort1 = ReadUInt16(buffer);
        DataListLength = ReadUInt32(buffer);
        var UnknownInt1 = ReadUInt32(buffer);
    }

    public virtual void ReadData(IBuffer buffer)
    {
        for (var i = 0; i <= DataListLength - 2; i++)
        {
            var abilityData = new AbilityData();
            abilityData.ReadAbilityData(buffer);
            DataList.Add(abilityData);
        }
    }
}
