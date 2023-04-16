using System;
using System.Collections.Generic;
using Arrowgene.Buffers;

namespace Arrowgene.Ddon.Client.Resource.Job.AbilityList.v8;

public class AbilityParam
{
    public int ParamType { get; set; }

    // Custom attribute for user friendly name resolution
    public string ParamTypeName { get; set; }

    public int CorrectType { get; set; }

    // Custom attribute for user friendly name resolution
    public string CorrectTypeName { get; set; }

    public uint ParamDataArrayLength { get; set; }

    public List<ParamData> ParamDataArray { get; set; }

    public static int ParamTypeMinimumValue()
    {
        return (int)AbilityParamTypeEnum.PARAM_TYPE_NONE;
    }

    public virtual int ParamTypeMaximumValue()
    {
        return (int)AbilityParamTypeEnum.PARAM_TYPE_DEF_ITEM_SEAL;
    }

    public virtual string GetParamTypeName(int paramType)
    {
        if (Enum.IsDefined(typeof(AbilityParamTypeEnum), paramType)) return ((AbilityParamTypeEnum)paramType).ToString();

        return null;
    }

    public void ReadAbilityParam(IBuffer buffer)
    {
        ParamType = buffer.ReadInt32(Endianness.Little);
        if (ParamType < ParamTypeMinimumValue() || ParamType > ParamTypeMaximumValue()) throw new Exception($"ParamType is outside of expected range, found {ParamType}!");

        ParamTypeName = GetParamTypeName(ParamType);

        CorrectType = buffer.ReadInt32(Endianness.Little);
        if (CorrectType is < 0 or > 1) throw new Exception($"CorrectType must be within range 0-1, found {CorrectType}!");

        CorrectTypeName = ((AbilityParamCorrectTypeEnum)CorrectType).ToString();

        ParamDataArrayLength = buffer.ReadUInt32(Endianness.Little);
        ParamDataArray = new List<ParamData>((int)ParamDataArrayLength);
        ParamDataArray.Clear();
        ReadParamDataArray(buffer);
    }

    public virtual void ReadParamDataArray(IBuffer buffer)
    {
        for (var i = 0; i < ParamDataArrayLength; i++)
        {
            var paramData = new ParamData();
            paramData.ReadParamData(buffer);
            ParamDataArray.Add(paramData);
        }
    }
}
