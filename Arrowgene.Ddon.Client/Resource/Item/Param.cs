using System;
using Arrowgene.Buffers;

namespace Arrowgene.Ddon.Client.Resource.Item;

public class Param
{
    public enum CRAFT_COLOR
    {
        CRAFT_COLOR_START = 0x1,
        CRAFT_COLOR_ALL = 0x1,
        CRAFT_COLOR_DEFAULT = 0x2,
        CRAFT_COLOR_RED = 0x3,
        CRAFT_COLOR_GREEN = 0x4,
        CRAFT_COLOR_BLUE = 0x5,
        CRAFT_COLOR_YELLOW = 0x6,
        CRAFT_COLOR_PINK = 0x7,
        CRAFT_COLOR_BLACK = 0x8,
        CRAFT_COLOR_END = 0x9,
        CRAFT_COLOR_NUM = 0x8
    }

    public enum ELEMENT_PARAM_KIND
    {
        KIND_NONE_ELEMENT = 0x0,
        WEIGHT_DOWN_ELEMENT = 0x1,
        SpredSav_UP_ELEMENT = 0x2,
        FreezeSav_UP_ELEMENT = 0x3,
        ShockSav_UP_ELEMENT = 0x4,
        CrossSav_UP_ELEMENT = 0x5,
        BlindSav_UP_ELEMENT = 0x6,
        ATTACK_UP_ELEMENT = 0x7,
        MAGICATTACK_UP_ELEMENT = 0x8,
        POWERREV_UP_ELEMENT = 0x9,
        StanSav_UP_ELEMENT = 0xA,
        PoisonSav_UP_ELEMENT = 0xB,
        SlowSav_UP_ELEMENT = 0xC,
        SleepSav_UP_ELEMENT = 0xD,
        WaterSav_UP_ELEMENT = 0xE,
        OilSav_UP_ELEMENT = 0xF,
        SealSav_UP_ELEMENT = 0x10,
        SoftBodySav_UP_ELEMENT = 0x11,
        StoneSav_UP_ELEMENT = 0x12,
        GoldSav_UP_ELEMENT = 0x13,
        FallFireSav_UP_ELEMENT = 0x14,
        FallIceSav_UP_ELEMENT = 0x15,
        FallThunderSav_UP_ELEMENT = 0x16,
        FallSaintSav_UP_ELEMENT = 0x17,
        FallBlindSav_UP_ELEMENT = 0x18,
        FallAttackSav_UP_ELEMENT = 0x19,
        FallDefSav_UP_ELEMENT = 0x1A,
        FallMagicSav_UP_ELEMENT = 0x1B,
        FallMagicDefSav_UP_ELEMENT = 0x1C,
        DEFENCE_UP_ELEMENT = 0x1D,
        MAGICDEFENSE_UP_ELEMENT = 0x1E,
        DURABILITY_UP_ELEMENT = 0x1F,
        SPIRIT_UP_ELEMENT = 0x20,
        HP_UP_ELEMENT = 0x21,
        ST_UP_ELEMENT = 0x22,
        ShinRyokuRev_UP_ELEMENT = 0x23,
        FireEleDef_UP_ELEMENT = 0x24,
        IceEleDef_UP_ELEMENT = 0x25,
        ThunderEleDef_UP_ELEMENT = 0x26,
        SaintEleDef_UP_ELEMENT = 0x27,
        DarkEleDef_UP_ELEMENT = 0x28,
        SpredDef_UP_ELEMENT = 0x29,
        FreezeDef_UP_ELEMENT = 0x2A,
        ShockDef_UP_ELEMENT = 0x2B,
        CrossDef_UP_ELEMENT = 0x2C,
        BlindDef_UP_ELEMENT = 0x2D,
        PoisonDef_UP_ELEMENT = 0x2E,
        SlowDef_UP_ELEMENT = 0x2F,
        SleepDef_UP_ELEMENT = 0x30,
        StanDef_UP_ELEMENT = 0x31,
        WaterDef_UP_ELEMENT = 0x32,
        OilDef_UP_ELEMENT = 0x33,
        SealDef_UP_ELEMENT = 0x34,
        CurseDef_UP_ELEMENT = 0x35,
        SoftBodyDef_UP_ELEMENT = 0x36,
        StoneDef_UP_ELEMENT = 0x37,
        GoldDef_UP_ELEMENT = 0x38,
        FallFireDef_UP_ELEMENT = 0x39,
        FallIceDef_UP_ELEMENT = 0x3A,
        FallThunderDef_UP_ELEMENT = 0x3B,
        FallSaintDef_UP_ELEMENT = 0x3C,
        FallBlindDef_UP_ELEMENT = 0x3D,
        FallAttackDef_UP_ELEMENT = 0x3E,
        FallDefenceDef_UP_ELEMENT = 0x3F,
        FallMagicAttackDef_UP_ELEMENT = 0x40,
        FallMagicDefenceDef_UP_ELEMENT = 0x41,
        VsEm00_UP_ELEMENT = 0x42,
        VsEm01_UP_ELEMENT = 0x43,
        VsEm02_UP_ELEMENT = 0x44,
        VsEm03_UP_ELEMENT = 0x45,
        VsEm04_UP_ELEMENT = 0x46,
        VsEm05_UP_ELEMENT = 0x47,
        VsEm06_UP_ELEMENT = 0x48,
        VsEm07_UP_ELEMENT = 0x49,
        VsEm08_UP_ELEMENT = 0x4A,
        VsEm09_UP_ELEMENT = 0x4B,
        VsEm10_UP_ELEMENT = 0x4C,
        VsEm11_UP_ELEMENT = 0x4D,
        VsEm12_UP_ELEMENT = 0x4E,
        VsEm13_UP_ELEMENT = 0x4F,
        VsEm14_UP_ELEMENT = 0x50,
        Color_ELEMENT = 0x51,
        DASH_ST_UP = 0x52,
        JUMP_UP = 0x53,
        CLIME_SPD_UP = 0x54,
        AWAKENING_WEIGHT_LIGHTRY = 0x55,
        LOW_LV_EXP_UP = 0x56,
        ABILITY = 0x57,
        VsEm15_UP_ELEMENT = 0x58,
        ELEMENT_PARAM_KIND_NUM = 0x59
    }

    public enum PARAM_KIND
    {
        KIND_NONE = 0x0,
        HP_RECOVER = 0x1,
        ST_RECOVER = 0x2,
        POISON_CLEAR = 0x3,
        SlOW_CLEAR = 0x4,
        SLEEP_CLEAR = 0x5,
        STAN_CLEAR = 0x6,
        WATER_CLEAR = 0x7,
        OIL_CLEAR = 0x8,
        SEAL_CLEAR = 0x9,
        SOFTBODY_CLEAR = 0xA,
        STONE_CLEAR = 0xB,
        GOLD_CLEAR = 0xC,
        SPREAD_CLEAR = 0xD,
        FREEZE_CLEAR = 0xE,
        FALLFIRE_CLEAR = 0xF,
        FALLICE_CLEAR = 0x10,
        FALLTHUNDER_CLEAR = 0x11,
        FALLSAINT_CLEAR = 0x12,
        FALLBLIND_CLEAR = 0x13,
        FALLATTACK_CLEAR = 0x14,
        FALLDEF_CLEAR = 0x15,
        FALLMAGIC_CLEAR = 0x16,
        FALLMAGICDEF_CLEAR = 0x17,
        ATTACK_UP = 0x18,
        DEFENCE_UP = 0x19,
        MAGICATTACK_UP = 0x1A,
        MAGICDEFENSE_UP = 0x1B,
        POWERREV_UP = 0x1C,
        DURABILITY_UP = 0x1D,
        SPIRIT_UP = 0x1E,
        HP_UP = 0x1F,
        ENDURANCE_UP = 0x20,
        BLIND_CLEAR = 0x21,
        REVIVAL_ONE = 0x22,
        REVIVAL_THREE = 0x23,
        LANTERN_ON = 0x24,
        GOLD_CHANGE = 0x6E,
        RIM_CHANGE = 0x6F,
        DOGMA_CHANGE = 0x70,
        MEDAL_POISON = 0x71,
        MEDAL_SLEEP = 0x72,
        MEDAL_STAN = 0x73,
        MEDAL_FALLFIRE = 0x74,
        MEDAL_FALLICE = 0x75,
        MEDAL_FALLTHUNDER = 0x76,
        MEDAL_FALLSAINT = 0x77,
        MEDAL_FALLBLIND = 0x78,
        MEDAL_SEAL = 0x79,
        MEDAL_STONE = 0x7A,
        MEDAL_GOLD = 0x7B,
        CURRENCY = 0x7C,
        THUNDER_CLEAR = 0x7D,
        EROSION_CLEAR = 0x7E,
        EROSION_GUARD_UP = 0x7F,
        JOB_POINT = 0x80,
        AREA_POINT = 0x81,
        SKILL_LEARN = 0x82,
        ABILITY_LEARN = 0x83,
        PAWN_USE = 0x84,
        KIND_NUM = 0x85
    }

    public short KindType { get; set; }
    public string KindTypeName { get; set; }

    public object Parameters { get; set; }

    public static Param ReadParam(ItemList.ITEM_CATEGORY itemCategory, IBuffer buffer)
    {
        var param = new Param();
        param.KindType = buffer.ReadInt16();
        if (!Enum.IsDefined(typeof(PARAM_KIND), (int)param.KindType) && !Enum.IsDefined(typeof(ELEMENT_PARAM_KIND), (int)param.KindType))
            throw new Exception($"@{buffer.Position} KindType is unknown!");
        if (Enum.IsDefined(typeof(PARAM_KIND), (int)param.KindType)) param.KindTypeName = ((PARAM_KIND)param.KindType).ToString();
        if (itemCategory == ItemList.ITEM_CATEGORY.CATEGORY_ARMS)
        {
            if (Enum.IsDefined(typeof(ELEMENT_PARAM_KIND), (int)param.KindType)) param.KindTypeName = ((ELEMENT_PARAM_KIND)param.KindType).ToString();
        }

        switch ((PARAM_KIND)param.KindType)
        {
            case PARAM_KIND.JOB_POINT:
                var jpGet = new JP_GET();
                jpGet.JobId = buffer.ReadUInt16();
                jpGet.Point = buffer.ReadUInt16();
                jpGet.Padding = buffer.ReadUInt16();
                param.Parameters = jpGet;
                break;
            case PARAM_KIND.AREA_POINT:
                var apGet = new AP_GET();
                apGet.AreaId = buffer.ReadUInt16();
                apGet.Point = buffer.ReadUInt16();
                apGet.Padding = buffer.ReadUInt16();
                param.Parameters = apGet;
                break;
            case PARAM_KIND.SKILL_LEARN:
                param.Parameters = new SKILL_LEARNING
                {
                    JobId = buffer.ReadUInt16(),
                    SkillNo = buffer.ReadUInt16(),
                    Padding = buffer.ReadUInt16()
                };
                break;
            // case Param.PARAM_KIND.PAWN_USE:
            //     param.Parameters = new Param.ABILITY_ASSIGNMENT
            //     {
            //         AbilityNo = buffer.ReadUInt16(),
            //         Lv = buffer.ReadUInt16(),
            //         Padding = buffer.ReadUInt16()
            //     };
            // break;
            case PARAM_KIND.ABILITY_LEARN:
                param.Parameters = new ABILITY_LEARNING
                {
                    AbilityNo = buffer.ReadUInt16(),
                    Padding1 = buffer.ReadUInt16(),
                    Padding2 = buffer.ReadUInt16()
                };
                break;
            default:
                param.Parameters = new PARAM_OTHER
                {
                    ParamEffect1 = buffer.ReadUInt16(),
                    ParamEffect2 = buffer.ReadUInt16(),
                    ParamEffect3 = buffer.ReadUInt16()
                };
                break;
        }

        return param;
    }

    // Used when PARAM_KIND < 128 "JOB_POINT" at which point the other structs are used (Probably)
    public class PARAM_OTHER
    {
        public ushort ParamEffect1 { get; set; }
        public ushort ParamEffect2 { get; set; }
        public ushort ParamEffect3 { get; set; }
    }

    public class AP_GET
    {
        public ushort AreaId { get; set; }
        public ushort Point { get; set; }
        public ushort Padding { get; set; }
    }

    public class JP_GET
    {
        public ushort JobId { get; set; }
        public ushort Point { get; set; }
        public ushort Padding { get; set; }
    }

    public class ABILITY_ASSIGNMENT
    {
        public ushort AbilityNo { get; set; }
        public ushort Lv { get; set; }
        public ushort Padding { get; set; }
    }

    public class SKILL_LEARNING
    {
        public ushort JobId { get; set; }
        public ushort SkillNo { get; set; }
        public ushort Padding { get; set; }
    }

    public class ABILITY_LEARNING
    {
        public ushort AbilityNo { get; set; }
        public ushort Padding1 { get; set; }
        public ushort Padding2 { get; set; }
    }
}
