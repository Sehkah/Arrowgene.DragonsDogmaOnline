namespace Arrowgene.Ddon.Client.Resource.Item;

public class EquipParamS32
{
    public struct PARAM_1
    {
        int ValueS32;
    }

    public struct PARAM_2
    {
        uint ValueU32;
    }

    public struct PARAM
    {
        PARAM_1 _anon_0;
        PARAM_2 _anon_1;
    }

    public enum FORM_TYPE
    {
        FORM_TYPE_S32 = 0x0,
        FORM_TYPE_U32 = 0x1
    }
    
    public byte KindType { get; set; }
    public byte Form { get; set; }
    public PARAM Value { get; set; }
}
