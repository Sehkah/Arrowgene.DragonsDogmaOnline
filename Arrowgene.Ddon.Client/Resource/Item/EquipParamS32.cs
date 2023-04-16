namespace Arrowgene.Ddon.Client.Resource.Item;

public class EquipParamS32
{
    public enum FORM_TYPE
    {
        FORM_TYPE_S32 = 0x0,
        FORM_TYPE_U32 = 0x1
    }

    public byte KindType { get; set; }
    public byte Form { get; set; }
    public PARAM Value { get; set; }

    public struct PARAM_1
    {
        private int ValueS32;
    }

    public struct PARAM_2
    {
        private uint ValueU32;
    }

    public struct PARAM
    {
        private PARAM_1 _anon_0;
        private PARAM_2 _anon_1;
    }
}
