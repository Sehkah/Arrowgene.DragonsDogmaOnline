using System.Collections.Generic;
using Arrowgene.Buffers;

namespace Arrowgene.Ddon.Client.Resource.Job.AbilityList.v8;

public class AbilityData
{
    public uint ParamArrayLength;
    public List<AbilityParam> ParamArray { get; set; }

    public virtual void ReadAbilityData(IBuffer buffer)
    {
        ParamArrayLength = buffer.ReadUInt32(Endianness.Little);
        ParamArray = new List<AbilityParam>((int)ParamArrayLength);
        ParamArray.Clear();
        ReadAbilityParamArray(buffer);
    }

    public virtual void ReadAbilityParamArray(IBuffer buffer)
    {
        for (var i = 0; i < ParamArrayLength; i++)
        {
            var abilityParam = new AbilityParam();
            abilityParam.ReadAbilityParam(buffer);
            ParamArray.Add(abilityParam);
        }
    }
}
