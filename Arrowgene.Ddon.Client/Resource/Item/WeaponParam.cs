using System.Collections.Generic;

namespace Arrowgene.Ddon.Client.Resource.Item;

public class WeaponParam // => 50 bytes + (10 bytes * EquipParamS8Num)
{
    public uint ModelTagId { get; set; }
    public uint PowerRev { get; set; }
    public uint Chance { get; set; }
    public uint Defense { get; set; }
    public uint MagicDefense { get; set; }
    public uint Durability { get; set; }
    public uint Attack { get; set; }
    public uint MagicAttack { get; set; }
    public uint ShieldStagger { get; set; } // => 9*4=36
    public List<EquipParamS8> EquipParamS8List { get; set; }
    public ushort Weight { get; set; }
    public ushort MaxHpRev { get; set; }
    public ushort MaxStRev { get; set; } // => 3*2 = 6
    public byte WepCategory { get; set; }
    public byte ColorNo { get; set; }
    public byte Sex { get; set; }
    public byte ModelParts { get; set; }
    public byte EleSlot { get; set; }
    public byte PhysicalType { get; set; }
    public byte ElementType { get; set; }
    public byte EquipParamS8Num { get; set; } // => 8*1 =8
}
