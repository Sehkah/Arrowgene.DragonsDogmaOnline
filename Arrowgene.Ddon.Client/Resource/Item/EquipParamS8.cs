namespace Arrowgene.Ddon.Client.Resource.Item;

public class EquipParamS8
{
    public struct PARAM_1
    {
        sbyte ValueS8;
        byte PaddingS8;
    }

    public struct PARAM_2
    {
        byte ValueU8;
        byte PaddingU8;
    }

    public struct PARAM_3
    {
        short ValueS16;
    }

    public struct PARAM_4
    {
        ushort ValueU16;
    }

    public struct PARAM
    {
        PARAM_1 _anon_0;
        PARAM_2 _anon_1;
        PARAM_3 _anon_2;
        PARAM_4 _anon_3;
    }
    
    public enum FORM_TYPE
    {
        FORM_TYPE_S8 = 0x0,
        FORM_TYPE_U8 = 0x1,
        FORM_TYPE_S16 = 0x2,
        FORM_TYPE_U16 = 0x3,
    }
    
    public byte KindType { get; set; }
    public byte Form { get; set; }
    public PARAM Value { get; set; }
}
