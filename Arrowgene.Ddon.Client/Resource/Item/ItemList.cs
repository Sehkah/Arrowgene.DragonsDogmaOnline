using System;
using System.Collections.Generic;
using Arrowgene.Buffers;

namespace Arrowgene.Ddon.Client.Resource.Item;

public class ItemList : ResourceFile
{
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

    public uint Version { get; set; }
    public List<ItemParam> ItemParamList { get; set; }
    public uint ArrayDataNum { get; set; }
    public List<Param> ParamList { get; set; }
    public uint ArrayParamDataNum { get; set; }
    public List<VsEnemyParam> VsParamList { get; set; }
    public uint ArrayVsParamDataNum { get; set; }
    public List<WeaponParam> WeaponParamList { get; set; }
    public uint ArrayWeaponParamDataNum { get; set; }
    public List<ProtectorParam> ProtectParamList { get; set; }
    public uint ArrayProtectParamDataNum { get; set; }
    public List<EquipParamS8> EquipParamS8List { get; set; }
    public uint ArrayEquipParamS8DataNum { get; set; }
    public ushort IndexTbl { get; set; }
    public uint MaxId { get; set; }

    // 990174 bytes 3.4 | 
    protected override void ReadResource(IBuffer buffer)
    {
        // 68 3.4 | 58 2.3
        Version = ReadUInt32(buffer);

        // 3406 3.4 | 13459 2.3
        ArrayDataNum = ReadUInt32(buffer);
        // 104 * ItemParamNum 2.3 => rItemParam
        ItemParamList = new List<ItemParam>((int)ArrayDataNum);
        for (var i = 0; i < 1; i++) ItemParamList.Add(ReadItemParam(buffer));

        ArrayParamDataNum = ReadUInt32(buffer);
        ParamList = new List<Param>((int)ArrayParamDataNum);
        for (var i = 0; i < ArrayParamDataNum; i++) ;

        ArrayVsParamDataNum = ReadUInt32(buffer);
        VsParamList = new List<VsEnemyParam>((int)ArrayVsParamDataNum);
        for (var i = 0; i < ArrayVsParamDataNum; i++) ;

        ArrayWeaponParamDataNum = ReadUInt32(buffer);
        WeaponParamList = new List<WeaponParam>((int)ArrayWeaponParamDataNum);
        for (var i = 0; i < ArrayWeaponParamDataNum; i++) ;

        ArrayProtectParamDataNum = ReadUInt32(buffer);
        ProtectParamList = new List<ProtectorParam>((int)ArrayProtectParamDataNum);
        for (var i = 0; i < ArrayProtectParamDataNum; i++) ;

        ArrayEquipParamS8DataNum = ReadUInt32(buffer);
        EquipParamS8List = new List<EquipParamS8>((int)ArrayEquipParamS8DataNum);
        for (var i = 0; i < ArrayEquipParamS8DataNum; i++) ;

        IndexTbl = ReadUInt16(buffer);
        MaxId = ReadUInt32(buffer);
    }

    private static ItemParam ReadItemParam(IBuffer buffer)
    {
        var itemParam = new ItemParam();
        itemParam.ItemId = buffer.ReadUInt32();
        itemParam.NameId = buffer.ReadUInt32();
        itemParam.EquipCategories = new ItemParam.EQUIP_CATEGORIES();
        itemParam.EquipCategories.EquipCategory = buffer.ReadByte();
        itemParam.EquipCategories.Padding = buffer.ReadByte();
        itemParam.EquipCategories.EquipSubCategory = buffer.ReadUInt16();
        itemParam.Price = buffer.ReadUInt32();
        itemParam.SortNo = buffer.ReadUInt32();
        itemParam.NameSortNo = buffer.ReadUInt32();
        itemParam.AttackStatus = buffer.ReadUInt32();
        itemParam.IsUseJob = buffer.ReadUInt32();
        itemParam.ParamNum = buffer.ReadUInt32();
        itemParam.ItemParamList = new List<Param>((int)itemParam.ParamNum);
        for (var i = 0; i < itemParam.ParamNum; i++) itemParam.ItemParamList.Add(ReadParam(buffer));

        itemParam.VsEmNum = buffer.ReadUInt32();
        itemParam.VsEmList = new List<VsEnemyParam>((int)itemParam.VsEmNum);
        for (var i = 0; i < itemParam.VsEmNum; i++) itemParam.VsEmList.Add(ReadVsEnemyParam(buffer));

        itemParam.WeaponParam = ReadWeaponParam(buffer);
        itemParam.ProtectorParam = ReadProtectorParam(buffer);
        itemParam.Flag = buffer.ReadUInt16();
        itemParam.IconNo = buffer.ReadUInt16();
        itemParam.IsUseLv = buffer.ReadUInt16();
        itemParam.Category = buffer.ReadByte();
        if (itemParam.Category > (int)ITEM_CATEGORY.CATEGORY_CRAFT_RECIPE) throw new Exception("Category can not be bigger than maximum expected 7!");

        if (itemParam.Category == (int)ITEM_CATEGORY.CATEGORY_MATERIAL_ITEM)
            itemParam.EquipCategories.EquipCategoryName = ((MATERIAL_CATEGORY)itemParam.EquipCategories.EquipCategory).ToString();


        itemParam.CategoryName = ((ITEM_CATEGORY)itemParam.Category).ToString();
        itemParam.StackMax = buffer.ReadByte();
        if (itemParam.Category == (int)ITEM_CATEGORY.CATEGORY_MATERIAL_ITEM && itemParam.StackMax != 99)
            throw new Exception("Materials is not stackable up to 99, this is unexpected!");

        itemParam.Rank = buffer.ReadByte();
        itemParam.Grade = buffer.ReadByte();
        itemParam.IconColNo = buffer.ReadByte();
        return itemParam;
    }

    private static ProtectorParam ReadProtectorParam(IBuffer buffer)
    {
        var protectorParam = new ProtectorParam();
        protectorParam.ModelTagId = buffer.ReadUInt32();
        protectorParam.PowerRev = buffer.ReadUInt32();
        protectorParam.Chance = buffer.ReadUInt32();
        protectorParam.Defense = buffer.ReadUInt32();
        protectorParam.MagicDefense = buffer.ReadUInt32();
        protectorParam.Durability = buffer.ReadUInt32();
        protectorParam.Attack = buffer.ReadUInt32();
        protectorParam.MagicAttack = buffer.ReadUInt32();
        protectorParam.EquipParamS8List = new List<EquipParamS8>(); //TODO
        protectorParam.Weight = buffer.ReadUInt16();
        protectorParam.MaxHpRev = buffer.ReadUInt16();
        protectorParam.MaxStRev = buffer.ReadUInt16();
        protectorParam.ColorNo = buffer.ReadByte();
        protectorParam.Sex = buffer.ReadByte();
        protectorParam.ModelParts = buffer.ReadByte();
        protectorParam.EleSlot = buffer.ReadByte();
        protectorParam.EquipParamS8Num = buffer.ReadByte();
        return protectorParam;
    }

    private static WeaponParam ReadWeaponParam(IBuffer buffer)
    {
        var weaponParam = new WeaponParam();
        weaponParam.ModelTagId = buffer.ReadUInt32();
        weaponParam.PowerRev = buffer.ReadUInt32();
        weaponParam.Chance = buffer.ReadUInt32();
        weaponParam.Defense = buffer.ReadUInt32();
        weaponParam.MagicDefense = buffer.ReadUInt32();
        weaponParam.Durability = buffer.ReadUInt32();
        weaponParam.Attack = buffer.ReadUInt32();
        weaponParam.MagicAttack = buffer.ReadUInt32();
        weaponParam.ShieldStagger = buffer.ReadUInt32();
        weaponParam.EquipParamS8List = new List<EquipParamS8>(); // TODO
        weaponParam.Weight = buffer.ReadUInt16();
        weaponParam.MaxHpRev = buffer.ReadUInt16();
        weaponParam.MaxStRev = buffer.ReadUInt16();
        weaponParam.WepCategory = buffer.ReadByte();
        weaponParam.ColorNo = buffer.ReadByte();
        weaponParam.Sex = buffer.ReadByte();
        weaponParam.ModelParts = buffer.ReadByte();
        weaponParam.EleSlot = buffer.ReadByte();
        weaponParam.PhysicalType = buffer.ReadByte();
        weaponParam.ElementType = buffer.ReadByte();
        weaponParam.EquipParamS8Num = buffer.ReadByte();
        return weaponParam;
    }

    private static VsEnemyParam ReadVsEnemyParam(IBuffer buffer)
    {
        var vsEnemyParam = new VsEnemyParam();
        vsEnemyParam.KindType = buffer.ReadByte();
        vsEnemyParam.Param = buffer.ReadUInt16();
        return vsEnemyParam;
    }

    private static Param ReadParam(IBuffer buffer)
    {
        var param = new Param();
        param.KindType = buffer.ReadInt16();
        param.Parameters = new Param.PARAM();
        param.Parameters.Other = new Param.PARAM_OTHER
        {
            ParamEffect1 = buffer.ReadUInt16(),
            ParamEffect2 = buffer.ReadUInt16(),
            ParamEffect3 = buffer.ReadUInt16()
        };
        param.Parameters.Ap = new Param.AP_GET
        {
            AreaId = buffer.ReadUInt16(),
            Point = buffer.ReadUInt16(),
            Padding = buffer.ReadUInt16()
        };
        param.Parameters.Jp = new Param.JP_GET
        {
            JobId = buffer.ReadUInt16(),
            Point = buffer.ReadUInt16(),
            Padding = buffer.ReadUInt16()
        };
        param.Parameters.AbilityAssignment = new Param.ABILITY_ASSIGNMENT
        {
            AbilityNo = buffer.ReadUInt16(),
            Lv = buffer.ReadUInt16(),
            Padding = buffer.ReadUInt16()
        };
        param.Parameters.SkillLearning = new Param.SKILL_LEARNING
        {
            JobId = buffer.ReadUInt16(),
            SkillNo = buffer.ReadUInt16(),
            Padding = buffer.ReadUInt16()
        };
        param.Parameters.AbilityLearning = new Param.ABILITY_LEARNING
        {
            AbilityNo = buffer.ReadUInt16(),
            Padding1 = buffer.ReadUInt16(),
            Padding2 = buffer.ReadUInt16()
        };
        return param;
    }
}
