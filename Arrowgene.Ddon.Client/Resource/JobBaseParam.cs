using System.Collections.Generic;
using Arrowgene.Buffers;

namespace Arrowgene.Ddon.Client.Resource;

/**
 * rJobBaseParam : rTbl2<nJobParam::cJobInfo> : rTbl2Base : cResource
 */
public class JobBaseParam : ClientFile
{
    public Tbl2 Table { get; }

    public JobBaseParam()
    {
        Table = new Tbl2
        {
            Data = new List<JobInfo>()
        };
    }

    public class Tbl2
    {
        public uint DataVersion { get; set; }
        public uint DataNum { get; set; }
        public List<JobInfo> Data { get; init; }
    }

    /**
     * nJobParam::cJobInfo : MtObject
     */
    public class JobInfo
    {
        public ushort Lv { get; set; }
        public ulong Exp { get; set; }
        public uint Atk { get; set; }
        public uint Def { get; set; }
        public uint MAtk { get; set; }
        public uint MDef { get; set; }
        public uint Strength { get; set; }
        public uint DownPower { get; set; }
        public uint ShakePower { get; set; }
        public uint StanPower { get; set; }
        public uint Constitution { get; set; }
        public uint Guts { get; set; }
        public ulong JobPoint { get; set; }
        public ushort FireResist { get; set; }
        public ushort IceResist { get; set; }
        public ushort ThunderResist { get; set; }
        public ushort HolyResist { get; set; }
        public ushort DarkResist { get; set; }
        public byte SpreadResist { get; set; }
        public byte FreezeResist { get; set; }
        public byte ShockResist { get; set; }
        public byte AbsorbResist { get; set; }
        public byte DarkElmResist { get; set; }
        public byte PoisonResist { get; set; }
        public byte SlowResist { get; set; }
        public byte SleepResist { get; set; }
        public byte StunResist { get; set; }
        public byte WetResist { get; set; }
        public byte OilResist { get; set; }
        public byte SealResist { get; set; }
        public byte CurseResist { get; set; }
        public byte SoftResist { get; set; }
        public byte StoneResist { get; set; }
        public byte GoldResist { get; set; }
        public byte FireReduceResist { get; set; }
        public byte IceReduceResist { get; set; }
        public byte ThunderReduceResist { get; set; }
        public byte HolyReduceResist { get; set; }
        public byte DarkReduceResist { get; set; }
        public byte AtkDownResist { get; set; }
        public byte DefDownResist { get; set; }
        public byte MAtkDownResist { get; set; }
        public byte MDefDownResist { get; set; }
        public byte ErosionResist { get; set; }
        public byte ItemSealResist { get; set; }
    }

    protected override void Read(IBuffer buffer)
    {
        Table.DataVersion = buffer.ReadUInt32();
        Table.DataNum = buffer.ReadUInt32();
        for (var i = 0; i < Table.DataNum; i++)
        {
            Table.Data.Add(ReadJobInfo(buffer));
        }
    }

    private static JobInfo ReadJobInfo(IBuffer buffer)
    {
        var data = new JobInfo
        {
            Lv = buffer.ReadUInt16(),
            Exp = buffer.ReadUInt64(),
            Atk = buffer.ReadUInt32(),
            Def = buffer.ReadUInt32(),
            MAtk = buffer.ReadUInt32(),
            MDef = buffer.ReadUInt32(),
            Strength = buffer.ReadUInt32(),
            DownPower = buffer.ReadUInt32(),
            ShakePower = buffer.ReadUInt32(),
            StanPower = buffer.ReadUInt32(),
            Constitution = buffer.ReadUInt32(),
            Guts = buffer.ReadUInt32(),
            JobPoint = buffer.ReadUInt64(),
            FireResist = buffer.ReadUInt16(),
            IceResist = buffer.ReadUInt16(),
            ThunderResist = buffer.ReadUInt16(),
            HolyResist = buffer.ReadUInt16(),
            DarkResist = buffer.ReadUInt16(),
            SpreadResist = buffer.ReadByte(),
            FreezeResist = buffer.ReadByte(),
            ShockResist = buffer.ReadByte(),
            AbsorbResist = buffer.ReadByte(),
            DarkElmResist = buffer.ReadByte(),
            PoisonResist = buffer.ReadByte(),
            SlowResist = buffer.ReadByte(),
            SleepResist = buffer.ReadByte(),
            StunResist = buffer.ReadByte(),
            WetResist = buffer.ReadByte(),
            OilResist = buffer.ReadByte(),
            SealResist = buffer.ReadByte(),
            CurseResist = buffer.ReadByte(),
            SoftResist = buffer.ReadByte(),
            StoneResist = buffer.ReadByte(),
            GoldResist = buffer.ReadByte(),
            FireReduceResist = buffer.ReadByte(),
            IceReduceResist = buffer.ReadByte(),
            ThunderReduceResist = buffer.ReadByte(),
            HolyReduceResist = buffer.ReadByte(),
            DarkReduceResist = buffer.ReadByte(),
            AtkDownResist = buffer.ReadByte(),
            DefDownResist = buffer.ReadByte(),
            MAtkDownResist = buffer.ReadByte(),
            MDefDownResist = buffer.ReadByte(),
            ErosionResist = buffer.ReadByte(),
            ItemSealResist = buffer.ReadByte()
        };
        return data;
    }
}
