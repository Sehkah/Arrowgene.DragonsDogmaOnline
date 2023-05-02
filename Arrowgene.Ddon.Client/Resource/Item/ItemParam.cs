using System.Collections.Generic;

namespace Arrowgene.Ddon.Client.Resource.Item;

public class ItemParam
{
    public enum CHAR_TYPE
    {
        CHAR_TYPE_NONE = 0,
        CHAR_TYPE_PL = 1,
        CHAR_TYPE_PAWN = 2,
        CHAR_TYPE_MAX = 3
    }

    public enum ELEMENT_TYPE
    {
        ELEMENT_TYPE_NONE = 0x0,
        ELEMENT_TYPE_FIRE = 0x1,
        ELEMENT_TYPE_ICE = 0x2,
        ELEMENT_TYPE_THUNDER = 0x3,
        ELEMENT_TYPE_SAINT = 0x4,
        ELEMENT_TYPE_DARK = 0x5,
        ELEMENT_TYPE_NUM = 0x6
    }

    public enum EQUIP_CATEGORY
    {
        EQUIP_CATEGORY_NONE = 0x0,
        EQUIP_CATEGORY_WEP_MAIN = 0x1,
        EQUIP_CATEGORY_WEP_SUB = 0x2,
        EQUIP_CATEGORY_ARMOR_HELM = 0x3,
        EQUIP_CATEGORY_ARMOR_BODY = 0x4,
        EQUIP_CATEGORY_WEAR_BODY = 0x5,
        EQUIP_CATEGORY_ARMOR_ARM = 0x6,
        EQUIP_CATEGORY_ARMOR_LEG = 0x7,
        EQUIP_CATEGORY_WEAR_LEG = 0x8,
        EQUIP_CATEGORY_ACCESSORY = 0x9,
        EQUIP_CATEGORY_JEWELRY = 0xA,
        EQUIP_CATEGORY_LANTERN = 0xB,
        EQUIP_CATEGORY_COSTUME = 0xC,
        EQUIP_CATEGORY_END = 0xD
    }

    public enum EQUIP_SUB_CATEGORY
    {
        EQUIP_SUB_CATEGORY_NONE = 0x0,
        EQUIP_SUB_CATEGORY_JEWELRY_COMMON = 0x1,
        EQUIP_SUB_CATEGORY_JEWELRY_RING = 0x2,
        EQUIP_SUB_CATEGORY_JEWELRY_BRACELET = 0x3,
        EQUIP_SUB_CATEGORY_JEWELRY_PIERCE = 0x4,
        EQUIP_SUB_CATEGORY_NUM = 0x4
    }

    public enum FLAG_TYPE
    {
        FLAG_TYPE_SELL = 0,
        FLAG_TYPE_BAZAAR = 1,
        FLAG_TYPE_UNUSE_LOBBY = 2,
        FLAG_TYPE_USE_NPC = 3,
        FLAG_TYPE_UNKNOWN_1 = 4,
        FLAG_TYPE_UNKNOWN_2 = 5,
        FLAG_TYPE_UNKNOWN_3 = 6,
        FLAG_TYPE_UNKNOWN_4 = 7,
        FLAG_TYPE_UNKNOWN_5 = 8,
        FLAG_TYPE_UNKNOWN_6 = 9,
        FLAG_TYPE_UNKNOWN_7 = 10,
        FLAG_TYPE_UNKNOWN_8 = 11,
        FLAG_TYPE_NUM = 12
    }

    public enum PHYSICAL_DEF_TYPE
    {
        PHYSICAL_DEF_TYPE_SWORD = 0x0,
        PHYSICAL_DEF_TYPE_HIT = 0x1,
        PHYSICAL_DEF_TYPE_ARROW = 0x2,
        PHYSICAL_DEF_TYPE_NUM = 0x3
    }

    public int Offset { get; set; }
    public uint ItemId { get; set; }

    public uint NameId { get; set; } // TODO ui\gui_cmn.arc -> ui\00_message\common\item_name.gmd

    // Description ->  TODO ui\item_info.arc -> ui\00_message\common\item_info.gmd	
    public ushort Category { get; set; }
    public string CategoryName { get; set; }
    public ushort SubCategory { get; set; }
    public string SubCategoryName { get; set; }
    public uint Price { get; set; }
    public uint SortNo { get; set; }
    public uint NameSortNo { get; set; }
    public uint AttackStatus { get; set; }
    public uint IsUseJob { get; set; }
    public ushort Flag { get; set; }
    public string FlagName { get; set; }
    public ushort IconNo { get; set; }
    public ushort IsUseLv { get; set; }
    public byte ItemCategory { get; set; }
    public string ItemCategoryName { get; set; }
    public byte StackMax { get; set; }
    public byte Rank { get; set; }
    public byte Grade { get; set; }
    public byte IconColNo { get; set; }
    public uint ParamNum { get; set; }
    public List<Param> ItemParamList { get; set; }
    public uint VsEmNum { get; set; }
    public List<VsEnemyParam> VsEmList { get; set; }
    public WeaponParam WeaponParam { get; set; }
    public ProtectorParam ProtectorParam { get; set; }
}
