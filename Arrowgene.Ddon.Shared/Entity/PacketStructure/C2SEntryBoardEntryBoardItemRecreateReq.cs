using System.Collections.Generic;
using Arrowgene.Buffers;
using Arrowgene.Ddon.Shared.Entity.Structure;
using Arrowgene.Ddon.Shared.Network;

namespace Arrowgene.Ddon.Shared.Entity.PacketStructure
{
    public class C2SEntryBoardEntryBoardItemRecreateReq : IPacketStructure
    {
        public PacketId Id => PacketId.C2S_ENTRY_BOARD_ENTRY_BOARD_ITEM_RECREATE_REQ;

        public C2SEntryBoardEntryBoardItemRecreateReq()
        {
            Password = string.Empty;
            Param = new CDataEntryItemParam();
        }

        public ulong BoardId {  get; set; }
        public string Password {  get; set; }
        public CDataEntryItemParam Param { get; set; }

        public class Serializer : PacketEntitySerializer<C2SEntryBoardEntryBoardItemRecreateReq>
        {
            public override void Write(IBuffer buffer, C2SEntryBoardEntryBoardItemRecreateReq obj)
            {
                WriteUInt64(buffer, obj.BoardId);
                WriteMtString(buffer, obj.Password);
                WriteEntity(buffer, obj.Param);
            }

            public override C2SEntryBoardEntryBoardItemRecreateReq Read(IBuffer buffer)
            {
                C2SEntryBoardEntryBoardItemRecreateReq obj = new C2SEntryBoardEntryBoardItemRecreateReq();
                obj.BoardId = ReadUInt64(buffer);
                obj.Password = ReadMtString(buffer);
                obj.Param = ReadEntity<CDataEntryItemParam>(buffer);
                return obj;
            }
        }
    }
}