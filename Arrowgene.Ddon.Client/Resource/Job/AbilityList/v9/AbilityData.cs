using Arrowgene.Buffers;

namespace Arrowgene.Ddon.Client.Resource.Job.AbilityList.v9;

public class AbilityData : v8.AbilityData
{
    public int UnknownParam1 { get; set; }
    public int UnknownParam2 { get; set; }

    public override void ReadAbilityData(IBuffer buffer)
    {
        base.ReadAbilityData(buffer);
        var additionalDataAvailable = buffer.ReadByte();
        if (additionalDataAvailable != 0)
        {
            UnknownParam1 = buffer.ReadInt32(Endianness.Little);
            UnknownParam2 = buffer.ReadInt32(Endianness.Little);
        }
    }

    public override void ReadAbilityParamArray(IBuffer buffer)
    {
        for (var i = 0; i < ParamArrayLength; i++)
        {
            var abilityParam = new AbilityParam();
            abilityParam.ReadAbilityParam(buffer);
            ParamArray.Add(abilityParam);
        }
    }
}
