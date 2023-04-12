namespace Arrowgene.Ddon.Client.Resource.Item;

public class WeaponParam
{
    public uint ModelTagId { get; set; }
    public uint PowerRev { get; set; }
    public uint Chance { get; set; }
    public uint Defense { get; set; }
    public uint MagicDefense { get; set; }
    public uint Durability { get; set; }
    public uint Attack { get; set; }
    public uint MagicAttack { get; set; }
    public uint ShieldStagger { get; set; }
    public EquipParamS8 EquipParamS8List { get; set; }
    public ushort Weight { get; set; }
    public ushort MaxHpRev { get; set; }
    public ushort MaxStRev { get; set; }
    public byte WepCategory { get; set; }
    public byte ColorNo { get; set; }
    public byte Sex { get; set; }
    public byte ModelParts { get; set; }
    public byte EleSlot { get; set; }
    public byte PhysicalType { get; set; }
    public byte ElementType { get; set; }
    public byte EquipParamS8Num { get; set; }
}
