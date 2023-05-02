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
        CATEGORY_SPECIAL = 0x8, // Profile Background, 
        CATEGORY_SPECIAL_PAWN = 0x9, //  Character Edit Parts (from Pawn) 
        CATEGORY_SPECIAL_EMOTE = 0xA, // Emote (from Pawn)
        CATEGORY_SPECIAL_UNKNOWN = 0xB, // not in use in 2.3
        CATEGORY_SPECIAL_CONVERSATION_DATA = 0xC // Pawn Conversation Data
    }

    public enum MATERIAL_CATEGORY
    {
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
    public uint ArrayParamDataNum { get; set; }
    public uint ArrayVsParamDataNum { get; set; }
    public uint ArrayWeaponParamDataNum { get; set; }
    public uint ArrayProtectParamDataNum { get; set; }
    public uint ArrayEquipParamS8DataNum { get; set; }

    protected override void ReadResource(IBuffer buffer)
    {
        // 68 3.4 | 58 2.3
        Version = ReadUInt32(buffer);

        // 3406 3.4 | 13459 2.3
        ArrayDataNum = ReadUInt32(buffer);
        ArrayParamDataNum = ReadUInt32(buffer);
        ArrayVsParamDataNum = ReadUInt32(buffer);
        ArrayWeaponParamDataNum = ReadUInt32(buffer);
        ArrayProtectParamDataNum = ReadUInt32(buffer);
        ArrayEquipParamS8DataNum = ReadUInt32(buffer);

        ItemParamList = new List<ItemParam>((int)ArrayDataNum);
        try
        {
            for (var i = 0; i < (int)ArrayDataNum; i++) ItemParamList.Add(ItemParam.ReadItemParam(buffer));
        }
        catch (Exception e)
        {
            Logger.Exception(e);
        }
    }
}
