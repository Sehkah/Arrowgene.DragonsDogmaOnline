using Arrowgene.Buffers;

namespace Arrowgene.Ddon.Client.Resource.Item;

public class ItemList : ResourceFile
{
    public enum USE_CATEGORY
    {
        USE_CATEGORY_DUMMY = 0x0,
        USE_CATEGORY_NONE = 0x1,
        USE_CATEGORY_THROW = 0x2,
        USE_CATEGORY_MINE = 0x3,
        USE_CATEGORY_LUMBER = 0x4,
        USE_CATEGORY_KEY = 0x5,
        USE_CATEGORY_JOBITEM = 0x6,
        USE_CATEGORY_UNUSE = 0x7,
        USE_CATEGORY_DOOR_KEY = 0x8,
        USE_CATEGORY_NUM = 0x9
    }

    public enum MATERIAL_CATEGORY
    {
        MATERIAL_CATEGORY_START = 0x0,
        MATERIAL_CATEGORY_NONE = 0x0,
        MATERIAL_CATEGORY_METAL = 0x1,
        MATERIAL_CATEGORY_STONE = 0x2,
        MATERIAL_CATEGORY_SAND = 0x3,
        MATERIAL_CATEGORY_CLOTH = 0x4,
        MATERIAL_CATEGORY_THREAD = 0x5,
        MATERIAL_CATEGORY_WOOL = 0x6,
        MATERIAL_CATEGORY_BARK = 0x7,
        MATERIAL_CATEGORY_BONE = 0x8,
        MATERIAL_CATEGORY_FANG = 0x9,
        MATERIAL_CATEGORY_HORN = 0xA,
        MATERIAL_CATEGORY_SHELL = 0xB,
        MATERIAL_CATEGORY_WING = 0xC,
        MATERIAL_CATEGORY_JEWEL = 0xD,
        MATERIAL_CATEGORY_GRASS = 0xE,
        MATERIAL_CATEGORY_FLOWER = 0xF,
        MATERIAL_CATEGORY_NUTS = 0x10,
        MATERIAL_CATEGORY_MUSHROOM = 0x11,
        MATERIAL_CATEGORY_WOODCHIP = 0x12,
        MATERIAL_CATEGORY_LIQUID = 0x13,
        MATERIAL_CATEGORY_BANDEROLE = 0x14,
        MATERIAL_CATEGORY_ALCHE = 0x15,
        MATERIAL_CATEGORY_MEAT = 0x16,
        MATERIAL_CATEGORY_OTHER = 0x17,
        MATERIAL_CATEGORY_ELEMENT_WEP = 0x18,
        MATERIAL_CATEGORY_ELEMENT_ARMOR = 0x19,
        MATERIAL_CATEGORY_SPECIAL_WEP = 0x1A,
        MATERIAL_CATEGORY_SPECIAL_ARMOR = 0x1B,
        MATERIAL_CATEGORY_COLOR = 0x1C,
        MATERIAL_CATEGORY_APPRAISAL = 0x1D,
        MATERIAL_CATEGORY_SPECIALTY_GOODS = 0x1E,
        MATERIAL_CATEGORY_NUM = 0x1F
    }

    public enum SEX_TYPE
    {
        SEX_TYPE_NONE = 0x0,
        SEX_TYPE_BOTH = 0x1,
        SEX_TYPE_MAN = 0x2,
        SEX_TYPE_WOMAN = 0x3,
        SEX_TYPE_NUM = 0x4
    }

    public enum KIND_TYPE
    {
        KIND_TYPE_NONE = 0x0,
        KIND_TYPE_S8_START = 0x1,
        KIND_TYPE_POISON_DEF = 0x1,
        KIND_TYPE_SLOW_DEF = 0x2,
        KIND_TYPE_OIL_DEF = 0x3,
        KIND_TYPE_BLIND_DEF = 0x4,
        KIND_TYPE_SLEEP_DEF = 0x5,
        KIND_TYPE_WATER_DEF = 0x6,
        KIND_TYPE_SEAL_DEF = 0x7,
        KIND_TYPE_SOFTBODY_DEF = 0x8,
        KIND_TYPE_STONE_DEF = 0x9,
        KIND_TYPE_GOLD_DEF = 0xA,
        KIND_TYPE_SPREAD_DEF = 0xB,
        KIND_TYPE_FROZEN_DEF = 0xC,
        KIND_TYPE_SHOCK_DEF = 0xD,
        KIND_TYPE_SAINT_DEF = 0xE,
        KIND_TYPE_SWOON_DEF = 0xF,
        KIND_TYPE_CURSE_DEF = 0x10,
        KIND_TYPE_DONW_FIRE = 0x11,
        KIND_TYPE_DOWN_ICE = 0x12,
        KIND_TYPE_DOWN_THUNDER = 0x13,
        KIND_TYPE_DOWN_SAINT = 0x14,
        KIND_TYPE_DOWN_BLIND = 0x15,
        KIND_TYPE_DOWN_ATTACK = 0x16,
        KIND_TYPE_DOWN_DEFENCE = 0x17,
        KIND_TYPE_DOWN_MAGIC_AT = 0x18,
        KIND_TYPE_DOWN_MAGIC_DEF = 0x19,
        KIND_TYPE_EROSION_DEF = 0x1A,
        KIND_TYPE_ITEMSEAL_DEF = 0x1B,
        KIND_TYPE_S8_END = 0x1C,
        KIND_TYPE_S8_NUM = 0x1B,
        KIND_TYPE_U8_START = 0x1C,
        KIND_TYPE_SPIRIT = 0x1C,
        KIND_TYPE_SHIELD_STAMINA = 0x1D,
        KIND_TYPE_TSHIELD_STORAGE = 0x1E,
        KIND_TYPE_ARROW_NUM = 0x1F,
        KIND_TYPE_U8_END = 0x20,
        KIND_TYPE_U8_NUM = 0x4,
        KIND_TYPE_S16_START = 0x20,
        KIND_TYPE_FIRE_ELE_DEF = 0x20,
        KIND_TYPE_ICE_ELE_DEF = 0x21,
        KIND_TYPE_THUNDER_ELE_DEF = 0x22,
        KIND_TYPE_SAINT_ELE_DEF = 0x23,
        KIND_TYPE_DARK_ELE_DEF = 0x24,
        KIND_TYPE_S16_END = 0x25,
        KIND_TYPE_S16_NUM = 0x5,
        KIND_TYPE_U16_START = 0x25,
        KIND_TYPE_POISON_SAV = 0x25,
        KIND_TYPE_SLOW_SAV = 0x26,
        KIND_TYPE_OIL_SAV = 0x27,
        KIND_TYPE_BLIND_SAV = 0x28,
        KIND_TYPE_SLEEP_SAV = 0x29,
        KIND_TYPE_WATER_SAV = 0x2A,
        KIND_TYPE_SEAL_SAV = 0x2B,
        KIND_TYPE_SOFTBODY_SAV = 0x2C,
        KIND_TYPE_STONE_SAV = 0x2D,
        KIND_TYPE_GOLD_SAV = 0x2E,
        KIND_TYPE_SPRED_SAV = 0x2F,
        KIND_TYPE_FREEZE_SAV = 0x30,
        KIND_TYPE_SHOCK_SAV = 0x31,
        KIND_TYPE_CROSS_SAV = 0x32,
        KIND_TYPE_DOWN_FIRE_SAV = 0x33,
        KIND_TYPE_DOWN_ICE_SAV = 0x34,
        KIND_TYPE_DOWN_THUNDER_SAV = 0x35,
        KIND_TYPE_DOWN_SAINT_SAV = 0x36,
        KIND_TYPE_DOWN_BLIND_SAV = 0x37,
        KIND_TYPE_DOWN_ATTACK_SAV = 0x38,
        KIND_TYPE_DOWN_DEF_SAV = 0x39,
        KIND_TYPE_DOWN_MAGIC_SAV = 0x3A,
        KIND_TYPE_DOWN_MAGIC_DEF_SAV = 0x3B,
        KIND_TYPE_STAN_SAV = 0x3C,
        KIND_TYPE_U16_END = 0x3D,
        KIND_TYPE_U16_NUM = 0x18
    }

    public enum ITEM_CATEGORY
    {
        CATEGORY_NONE = 0x0,
        CATEGORY_USE_ITEM = 0x1,
        CATEGORY_MATERIAL_ITEM = 0x2,
        CATEGORY_ARMS = 0x3,
        CATEGORY_KEY_ITEM = 0x4,
        CATEGORY_JOB_ITEM = 0x5,
        CATEGORY_FURNITURE = 0x6,
        CATEGORY_CRAFT_RECIPE = 0x7,
        CATEGORY_NUM = 0x8
    }
    
    public ItemParam ItemParamList { get; set; }
    public uint ArrayDataNum { get; set; }
    public Param ParamList { get; set; }
    public uint ArrayParamDataNum { get; set; }
    public VsEnemyParam VsParamList { get; set; }
    public uint ArrayVsParamDataNum { get; set; }
    public WeaponParam WeaponParamList { get; set; }
    public uint ArrayWeaponParamDataNum { get; set; }
    public ProtectorParam ProtectParamList { get; set; }
    public uint ArrayProtectParamDataNum { get; set; }
    public EquipParamS8 EquipParamS8List { get; set; }
    public uint ArrayEquipParamS8DataNum { get; set; }
    public ushort IndexTbl { get; set; }
    public uint MaxId { get; set; }

    public ushort Version { get; set; }
    public uint Count { get; set; }

    public ItemList()
    {
    }

    // 990174 bytes
    protected override void ReadResource(IBuffer buffer)
    {
        // 68
        Version = ReadUInt16(buffer);
        // 3406
        Count = ReadUInt32(buffer);
        // 104 * Count = 354224 bytes
        for (var i = 0; i < Count; i++)
        {
        }
    }
}
