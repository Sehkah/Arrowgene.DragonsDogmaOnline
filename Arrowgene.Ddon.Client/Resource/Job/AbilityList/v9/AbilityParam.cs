using System;

namespace Arrowgene.Ddon.Client.Resource.Job.AbilityList.v9;

public class AbilityParam : v8.AbilityParam
{
    public override int ParamTypeMaximumValue()
    {
        return (int)AbilityParamTypeEnum.PARAM_TYPE_UNKNOWN_39;
    }

    public override string GetParamTypeName(int paramType)
    {
        var paramTypeName = base.GetParamTypeName(paramType);
        if (paramTypeName != null) return paramTypeName;
        return Enum.IsDefined(typeof(AbilityParamTypeEnum), paramType) ? ((AbilityParamTypeEnum)paramType).ToString() : null;
    }
}
