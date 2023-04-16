using Arrowgene.Buffers;

namespace Arrowgene.Ddon.Client.Resource.Job.AbilityList.v9;

public class AbilityList : v8.AbilityList
{
    public byte UnknownByte;

    public override void ReadHeader(IBuffer buffer)
    {
        base.ReadHeader(buffer);
        UnknownByte = ReadByte(buffer);
    }

    public override void ReadData(IBuffer buffer)
    {
        for (var i = 0; i <= DataListLength - 2; i++)
        {
            var abilityData = new AbilityData();
            abilityData.ReadAbilityData(buffer);
            DataList.Add(abilityData);
        }
    }
}
