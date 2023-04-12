namespace Arrowgene.Ddon.Client.Resource.Item;

public class ItemParam
{
    public struct EQUIP_CATEGORIES
    {
        byte EquipCategory;
        byte Padding;
        ushort EquipSubCategory;
    }

    public struct SUB_CATEGORY
    {
        EQUIP_CATEGORIES EquipCategories;
        ItemList.USE_CATEGORY UseCategory;
        ItemList.MATERIAL_CATEGORY MaterialCategory;
        uint Category;
    }

    public enum EQUIP_SUB_CATEGORY
    {
        EQUIP_SUB_CATEGORY_NONE = 0x0,
        EQUIP_SUB_CATEGORY_TOP = 0x1,
        EQUIP_SUB_CATEGORY_JEWELRY_COMMON = 0x1,
        EQUIP_SUB_CATEGORY_JEWELRY_RING = 0x2,
        EQUIP_SUB_CATEGORY_JEWELRY_BRACELET = 0x3,
        EQUIP_SUB_CATEGORY_JEWELRY_PIERCE = 0x4,
        EQUIP_SUB_CATEGORY_MAX = 0x5,
        EQUIP_SUB_CATEGORY_NUM = 0x4,
    }

    public enum ELEMENT_TYPE
    {
        ELEMENT_TYPE_NONE = 0x0,
        ELEMENT_TYPE_FIRE = 0x1,
        ELEMENT_TYPE_ICE = 0x2,
        ELEMENT_TYPE_THUNDER = 0x3,
        ELEMENT_TYPE_SAINT = 0x4,
        ELEMENT_TYPE_DARK = 0x5,
        ELEMENT_TYPE_NUM = 0x6,
    }

    public enum PHYSICAL_TYPE
    {
        PHYSICAL_TYPE_SWORD = 0x0,
        PHYSICAL_TYPE_HIT = 0x1,
        PHYSICAL_TYPE_ARROW = 0x2,
        PHYSICAL_TYPE_NUM = 0x3,
    }

    public uint ItemId { get; set; }
    public uint NameId { get; set; }
    public EQUIP_CATEGORIES EquipCategories { get; set; }
    public uint Price { get; set; }
    public uint SortNo { get; set; }
    public uint NameSortNo { get; set; }
    public uint AttackStatus { get; set; }
    public uint IsUseJob { get; set; }
    public Param ItemParamList { get; set; }
    public uint ParamNum { get; set; }
    public VsEnemyParam VsEmList { get; set; }
    public uint VsEmNum { get; set; }
    public WeaponParam WeaponParam { get; set; }
    public ProtectorParam ProtectorParam { get; set; }
    public ushort Flag { get; set; }
    public ushort IconNo { get; set; }
    public ushort IsUseLv { get; set; }
    public byte Category { get; set; }
    public byte StackMax { get; set; }
    public byte Rank { get; set; }
    public byte Grade { get; set; }
    public byte IconColNo { get; set; }
}
