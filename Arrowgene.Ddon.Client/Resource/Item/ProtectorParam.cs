using System;
using System.Collections.Generic;
using Arrowgene.Buffers;

namespace Arrowgene.Ddon.Client.Resource.Item;

public class ProtectorParam
{
    public uint ModelTagId { get; set; }
    public uint PowerRev { get; set; }
    public uint Chance { get; set; }
    public uint Defense { get; set; }
    public uint MagicDefense { get; set; }
    public uint Durability { get; set; }
    public uint Attack { get; set; }
    public uint MagicAttack { get; set; }
    public ushort Weight { get; set; }
    public ushort MaxHpRev { get; set; }
    public ushort MaxStRev { get; set; }
    public byte ColorNo { get; set; }
    public byte Sex { get; set; }
    public string SexName { get; set; }
    public byte ModelParts { get; set; }
    public byte EleSlot { get; set; }
    public byte EquipParamS8Num { get; set; }
    public List<EquipParamS8> EquipParamS8List { get; set; }

    public static ProtectorParam ReadProtectorParam(IBuffer buffer)
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
        protectorParam.Weight = buffer.ReadUInt16();
        protectorParam.MaxHpRev = buffer.ReadUInt16();
        protectorParam.MaxStRev = buffer.ReadUInt16();
        protectorParam.ColorNo = buffer.ReadByte();
        protectorParam.Sex = buffer.ReadByte();

        if (!Enum.IsDefined(typeof(ItemList.SEX_TYPE), (int)protectorParam.Sex)) throw new Exception($"@{buffer.Position} Sex is unknown!");
        protectorParam.SexName = ((ItemList.SEX_TYPE)protectorParam.Sex).ToString();

        protectorParam.ModelParts = buffer.ReadByte();
        protectorParam.EleSlot = buffer.ReadByte();
        protectorParam.EquipParamS8Num = buffer.ReadByte();
        protectorParam.EquipParamS8List = new List<EquipParamS8>(protectorParam.EquipParamS8Num);
        for (var i = 0; i < protectorParam.EquipParamS8Num; i++) protectorParam.EquipParamS8List.Add(EquipParamS8.ReadEquipParamS8(buffer));
        return protectorParam;
    }
}
