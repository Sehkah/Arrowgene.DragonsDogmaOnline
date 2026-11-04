using Arrowgene.Buffers;
using Arrowgene.Ddon.Shared.Model;
using Arrowgene.Ddon.Shared.Network;

namespace Arrowgene.Ddon.Shared.Entity.PacketStructure
{
    public class C2SConnectionMoveInServerReq : IPacketStructure
    {
        public string SessionKey { get; set; } = string.Empty;
        public PlatformType PlatformType { get; set; }
        public uint ClientVersion { get; set; }

        public PacketId Id => PacketId.C2S_CONNECTION_MOVE_IN_SERVER_REQ;
        
        public class Serializer : PacketEntitySerializer<C2SConnectionMoveInServerReq>
        {

            public override void Write(IBuffer buffer, C2SConnectionMoveInServerReq obj)
            {
                WriteMtString(buffer, obj.SessionKey);
                WriteByte(buffer, (byte)obj.PlatformType);
                WriteUInt32(buffer, obj.ClientVersion);
            }

            public override C2SConnectionMoveInServerReq Read(IBuffer buffer)
            {
                C2SConnectionMoveInServerReq obj = new C2SConnectionMoveInServerReq();
                obj.SessionKey = ReadMtString(buffer);
                obj.PlatformType = (PlatformType)ReadByte(buffer);
                obj.ClientVersion = ReadUInt32(buffer);
                return obj;
            }
        }
    }
}
