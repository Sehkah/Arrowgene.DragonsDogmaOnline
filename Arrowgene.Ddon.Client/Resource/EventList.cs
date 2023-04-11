using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;
using Arrowgene.Buffers;

namespace Arrowgene.Ddon.Client.Resource;

/**
 * rEventParam : rTbl2<cEventParam> : rTbl2Base : cResource
 */
public class EventList : ClientFile
{
    public Tbl2 Table { get; }

    public EventList()
    {
        Table = new Tbl2
        {
            Data = new List<EventParam>()
        };
    }

    public class Tbl2
    {
        public uint DataVersion { get; set; }
        public uint DataNum { get; set; }
        public List<EventParam> Data { get; init; }
    }

    /**
     * cStatusGain : MtObject
     */
    public class EventParam
    {
        public ushort Type { get; set; }
        // Custom attribute for user friendly name resolution
        public string TypeName { get; set; }
        public ushort Stage { get; set; }
        public ushort EvNo { get; set; }
        public ushort Flag { get; set; }
        // Custom attribute for user friendly name resolution
        public string FlagName { get; set; }
        public string FileName { get; set; }
        public uint QuestId { get; set; }
        public uint LightCtrl { get; set; }
        // Custom attribute for user friendly name resolution
        public string LightCtrlName { get; set; }
        public byte StartFadeType { get; set; }
        public byte EndFadeType { get; set; }
        public short SubMixerBefore { get; set; }
        public short SubMixerAfter { get; set; }
        public List<OmList> OmList { get; set; }
        public float OmAQCScale { get; set; }
        public uint Version { get; set; }
    }

    public class OmList
    {
        public uint OmId { get; set; }
        public ushort CtrlType { get; set; }
        // Custom attribute for user friendly name resolution
        public string CtrlTypeName { get; set; }
        public ushort LotType { get; set; }
        public short GroupNo { get; set; }
        public short SetId { get; set; }
    }

    protected override void Read(IBuffer buffer)
    {
        Table.DataVersion = ReadUInt32(buffer);
        Table.DataNum = ReadUInt32(buffer);
        for (var i = 0; i < Table.DataNum; i++)
        {
            Table.Data.Add(ReadEventParam(buffer));
        }
    }

    protected override void Write(IBuffer buffer)
    {
        throw new NotImplementedException();
    }

    private EventParam ReadEventParam(IBuffer buffer)
    {
        var data = new EventParam();
        data.Type = ReadUInt16(buffer);
        data.TypeName = ((EVENT_TYPE)data.Type).ToString();
        data.Stage = ReadUInt16(buffer);
        data.EvNo = ReadUInt16(buffer);
        data.Flag = ReadUInt16(buffer);
        // Custom attribute
        data.FlagName = ((EVENT_FLAG)data.Flag).ToString();
        data.FileName = buffer.ReadCString(Encoding.UTF8);
        if (!data.FileName.StartsWith("event"))
        {
            throw new Exception($"Event does not start with 'event'! pos: {buffer.Position}");
        }
        data.QuestId = ReadUInt32(buffer);
        data.LightCtrl = ReadUInt32(buffer);
        data.LightCtrlName = ((LIGHT_CTRL_TYPE)data.LightCtrl).ToString();
        data.StartFadeType = ReadByte(buffer);
        data.EndFadeType = ReadByte(buffer);
        data.SubMixerBefore = ReadInt16(buffer);
        data.SubMixerAfter = ReadInt16(buffer);
        data.OmAQCScale = ReadFloat(buffer);
        if (float.IsNaN(data.OmAQCScale))
        {
            throw new Exception($"OmAQCScale can not be NaN! pos: {buffer.Position}");
        }
        // OmList is an array, but if it's not in use the structure will not be reflected at all and the next attribute will immediately be the version,
        // thus sharing the same upper bytes
        byte[] versionAndLength = buffer.ReadBytes(4);
        data.Version = BinaryPrimitives.ReadUInt32LittleEndian(versionAndLength);
        byte len = versionAndLength[0];
        if (len > 0)
        {
            data.OmList = new List<OmList>(len);
            for (int i = 0; i < len; i++)
            {
                data.OmList.Add(ReadOmList(buffer));
            }
        }

        return data;
    }

    private OmList ReadOmList(IBuffer buffer)
    {
        var omList = new OmList();
        omList.OmId = ReadUInt32(buffer);
        omList.CtrlType = ReadUInt16(buffer);
        omList.CtrlTypeName = ((OM_CTRL_TYPE)omList.CtrlType).ToString();
        omList.LotType = ReadUInt16(buffer);
        omList.GroupNo = ReadInt16(buffer);
        omList.SetId = ReadInt16(buffer);
        return omList;
    }

    enum EVENT_FLAG
    {
        FLAG_NONE = 0x0,
        FLAG_LIGHT1 = 0x1,
        FLAG_DUMMY_0 = 0x2,
        FLAG_NO_FSM_SDL = 0x4,
        FLAG_NO_PARTY = 0x8,
        FLAG_CHG_SUB_MIXER = 0x10,
        FLAG_ON_STG_BGM = 0x20,
        FLAG_ON_BTL_BGM = 0x40,
    }

    enum EVENT_TYPE
    {
        TYPE_CUTIN = 0x0,
        TYPE_MOVIE = 0x1,
        TYPE_FSM = 0x2,
    }

    enum OM_CTRL_TYPE
    {
        OM_CTRL_NORMAL = 0x0,
        OM_CTRL_NO_DISP = 0x1,
        OM_CTRL_NO_SET = 0x2,
        OM_CTRL_UPDATE = 0x3,
        OM_CTRL_ARC_LOAD = 0x4,
    }

    enum LIGHT_CTRL_TYPE
    {
        LIGHT_CTRL_NONE = 0x0,
        LIGHT_CTRL_ALL = 0x1,
        LIGHT_CTRL_NO_NIGHT = 0x2,
        LIGHT_CTRL_ONLY_NIGHT = 0x3,
    }
}
