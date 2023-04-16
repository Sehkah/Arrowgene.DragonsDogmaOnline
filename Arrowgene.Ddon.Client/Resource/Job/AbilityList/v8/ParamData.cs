using Arrowgene.Buffers;

namespace Arrowgene.Ddon.Client.Resource.Job.AbilityList.v8;

public class ParamData
{
    public int Lv { get; set; }
    public int Param { get; set; }

    public void ReadParamData(IBuffer buffer)
    {
        Lv = buffer.ReadInt32(Endianness.Little);
        Param = buffer.ReadInt32(Endianness.Little);
    }
}
