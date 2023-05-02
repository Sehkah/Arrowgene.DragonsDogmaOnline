using System;
using Arrowgene.Buffers;

namespace Arrowgene.Ddon.Client.Resource.Item;

public class EquipParamS8
{
    public enum FORM_TYPE
    {
        FORM_TYPE_S8 = 0x0,
        FORM_TYPE_U8 = 0x1,
        FORM_TYPE_S16 = 0x2,
        FORM_TYPE_U16 = 0x3
    }

    public enum KIND_TYPE
    {
        KIND_TYPE_NONE = 0x0,
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
        KIND_TYPE_DOWN_FIRE = 0x11,
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
        KIND_TYPE_SPIRIT = 0x1C,
        KIND_TYPE_SHIELD_STAMINA = 0x1D,
        KIND_TYPE_TSHIELD_STORAGE = 0x1E,
        KIND_TYPE_ARROW_NUM = 0x1F,
        KIND_TYPE_FIRE_ELE_DEF = 0x20,
        KIND_TYPE_ICE_ELE_DEF = 0x21,
        KIND_TYPE_THUNDER_ELE_DEF = 0x22,
        KIND_TYPE_SAINT_ELE_DEF = 0x23,
        KIND_TYPE_DARK_ELE_DEF = 0x24,
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
        KIND_TYPE_STAN_SAV = 0x3C
    }

    public byte KindType { get; set; }
    public string KindTypeName { get; set; }
    public byte Form { get; set; }
    public object Parameter { get; set; }

    public static EquipParamS8 ReadEquipParamS8(IBuffer buffer)
    {
        var equipParam = new EquipParamS8();

        equipParam.KindType = buffer.ReadByte();

        if (!Enum.IsDefined(typeof(KIND_TYPE), (int)equipParam.KindType)) throw new Exception($"@{buffer.Position} KindType is unknown!");
        equipParam.KindTypeName = ((KIND_TYPE)equipParam.KindType).ToString();

        equipParam.Form = buffer.ReadByte();
        if (equipParam.Form > (int)FORM_TYPE.FORM_TYPE_U16) throw new Exception($"Equip Param Form can not be bigger than maximum expected {(int)FORM_TYPE.FORM_TYPE_U16}!");

        switch ((FORM_TYPE)equipParam.Form)
        {
            case FORM_TYPE.FORM_TYPE_S8:
                equipParam.Parameter = new PARAM_S8
                {
                    Value = (sbyte)buffer.ReadByte() // The StreamBuffer does not expose the underlying BinaryReader's ReadSByte function..
                };
                break;
            case FORM_TYPE.FORM_TYPE_U8:
                equipParam.Parameter = new PARAM_U8
                {
                    Value = buffer.ReadByte()
                };
                break;
            case FORM_TYPE.FORM_TYPE_S16:
                equipParam.Parameter = new PARAM_S16
                {
                    Value = buffer.ReadInt16()
                };
                break;
            case FORM_TYPE.FORM_TYPE_U16:
                equipParam.Parameter = new PARAM_U16
                {
                    Value = buffer.ReadUInt16()
                };
                break;
            default:
                throw new Exception("Unable to map EquipParamS8 type.");
        }

        return equipParam;
    }

    public class PARAM_S8
    {
        public sbyte Value { get; set; }
    }

    public class PARAM_U8
    {
        public byte Value { get; set; }
    }

    public class PARAM_S16
    {
        public short Value { get; set; }
    }

    public class PARAM_U16
    {
        public ushort Value { get; set; }
    }
}
