namespace Arrowgene.Ddon.Client.Resource.Item;

public class EquipParamS8 // => 10 bytes
{
    public enum FORM_TYPE
    {
        FORM_TYPE_S8 = 0x0,
        FORM_TYPE_U8 = 0x1,
        FORM_TYPE_S16 = 0x2,
        FORM_TYPE_U16 = 0x3
    }

    public byte KindType { get; set; }
    public byte Form { get; set; }
    public PARAM Value { get; set; }

    public struct PARAM_1
    {
        private sbyte ValueS8;
        private byte PaddingS8;
    }

    public struct PARAM_2
    {
        private byte ValueU8;
        private byte PaddingU8;
    }

    public struct PARAM_3
    {
        private short ValueS16;
    }

    public struct PARAM_4
    {
        private ushort ValueU16;
    }

    public class PARAM
    {
        private PARAM_1 _anon_0;
        private PARAM_2 _anon_1;
        private PARAM_3 _anon_2;
        private PARAM_4 _anon_3;
    }
}
