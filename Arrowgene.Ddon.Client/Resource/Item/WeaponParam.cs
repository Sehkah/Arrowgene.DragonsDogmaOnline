using System;
using System.Collections.Generic;
using Arrowgene.Buffers;

namespace Arrowgene.Ddon.Client.Resource.Item;

public class WeaponParam
{
    public enum PHYSICAL_TYPE
    {
        PHYSICAL_TYPE_SWORD = 0x0,
        PHYSICAL_TYPE_HIT = 0x1,
        PHYSICAL_TYPE_ARROW = 0x2,
        PHYSICAL_TYPE_NUM = 0x3
    }

    public enum WEAPON_CATEGORY
    {
        HAND = 0x0,
        SWORD = 0x1,
        SHIELD = 0x2,
        GSWORD = 0x3,
        SHIELD_L = 0x4,
        MACE = 0x5,
        DAGGER = 0x6,
        BOW = 0x7,
        GUN = 0x8,
        BOW_MG = 0x9,
        QUIVER = 0xA,
        WAND = 0xB,
        WAND_DX = 0xC,
        LANCE = 0xD,
        WIRE = 0xE,
        WEAPON_CATEGORY_NUM = 0xF
    }

    public uint ModelTagId { get; set; }
    public uint PowerRev { get; set; }
    public uint Chance { get; set; }
    public uint Defense { get; set; }
    public uint MagicDefense { get; set; }
    public uint Durability { get; set; }
    public byte WepCategory { get; set; }
    public string WepCategoryName { get; set; }
    public uint Attack { get; set; }
    public uint MagicAttack { get; set; }
    public uint ShieldStagger { get; set; }
    public ushort Weight { get; set; }
    public ushort MaxHpRev { get; set; }
    public ushort MaxStRev { get; set; }
    public byte ColorNo { get; set; }
    public byte Sex { get; set; }
    public string SexName { get; set; }
    public byte ModelParts { get; set; }
    public byte EleSlot { get; set; }
    public byte PhysicalType { get; set; }
    public string PhysicalTypeName { get; set; }
    public byte ElementType { get; set; }
    public string ElementTypeName { get; set; }
    public byte EquipParamS8Num { get; set; }
    public List<EquipParamS8> EquipParamS8List { get; set; }

    public static WeaponParam ReadWeaponParam(IBuffer buffer)
    {
        var weaponParam = new WeaponParam();
        weaponParam.ModelTagId = buffer.ReadUInt32();
        weaponParam.PowerRev = buffer.ReadUInt32();
        weaponParam.Chance = buffer.ReadUInt32();
        weaponParam.Defense = buffer.ReadUInt32();
        weaponParam.MagicDefense = buffer.ReadUInt32();
        weaponParam.Durability = buffer.ReadUInt32();

        weaponParam.WepCategory = buffer.ReadByte();
        if (!Enum.IsDefined(typeof(WEAPON_CATEGORY), (int)weaponParam.WepCategory)) throw new Exception($"@{buffer.Position} WepCategory is unknown!");
        weaponParam.WepCategoryName = ((WEAPON_CATEGORY)weaponParam.WepCategory).ToString();

        weaponParam.Attack = buffer.ReadUInt32();
        weaponParam.MagicAttack = buffer.ReadUInt32();
        weaponParam.ShieldStagger = buffer.ReadUInt32();
        weaponParam.Weight = buffer.ReadUInt16();
        weaponParam.MaxHpRev = buffer.ReadUInt16();
        weaponParam.MaxStRev = buffer.ReadUInt16();
        weaponParam.ColorNo = buffer.ReadByte();

        weaponParam.Sex = buffer.ReadByte();
        if (!Enum.IsDefined(typeof(ItemList.SEX_TYPE), (int)weaponParam.Sex)) throw new Exception($"@{buffer.Position} Sex is unknown!");
        weaponParam.SexName = ((ItemList.SEX_TYPE)weaponParam.Sex).ToString();

        weaponParam.ModelParts = buffer.ReadByte();
        weaponParam.EleSlot = buffer.ReadByte();

        weaponParam.PhysicalType = buffer.ReadByte();
        if (!Enum.IsDefined(typeof(PHYSICAL_TYPE), (int)weaponParam.PhysicalType)) throw new Exception($"@{buffer.Position} PhysicalType is unknown!");
        weaponParam.PhysicalTypeName = ((PHYSICAL_TYPE)weaponParam.PhysicalType).ToString();

        weaponParam.ElementType = buffer.ReadByte();
        if (!Enum.IsDefined(typeof(ItemParam.ELEMENT_TYPE), (int)weaponParam.ElementType)) throw new Exception($"@{buffer.Position} ElementType is unknown!");
        weaponParam.ElementTypeName = ((ItemParam.ELEMENT_TYPE)weaponParam.ElementType).ToString();

        weaponParam.EquipParamS8Num = buffer.ReadByte();
        weaponParam.EquipParamS8List = new List<EquipParamS8>(weaponParam.EquipParamS8Num);
        for (var i = 0; i < weaponParam.EquipParamS8Num; i++) weaponParam.EquipParamS8List.Add(EquipParamS8.ReadEquipParamS8(buffer));
        return weaponParam;
    }
}
