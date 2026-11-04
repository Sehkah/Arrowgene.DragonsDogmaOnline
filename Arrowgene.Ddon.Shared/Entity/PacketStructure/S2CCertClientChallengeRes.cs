using System;
using Arrowgene.Buffers;
using Arrowgene.Ddon.Shared.Network;

namespace Arrowgene.Ddon.Shared.Entity.PacketStructure
{
    public class S2CCertClientChallengeRes : ServerResponse
    {
        public S2CCertClientChallengeRes()
        {
            PasswordEnc = new byte[16];
        }

        public override PacketId Id => PacketId.S2C_CERT_CLIENT_CHALLENGE_RES;

        public byte PasswordSrcSize { get; set; }
        public byte PasswordEncSize { get; set; }
        public byte[] PasswordEnc { get; set; }

        public class Serializer : PacketEntitySerializer<S2CCertClientChallengeRes>
        {
            public override void Write(IBuffer buffer, S2CCertClientChallengeRes obj)
            {
                WriteServerResponse(buffer, obj);
                WriteByte(buffer, obj.PasswordSrcSize);
                WriteByte(buffer, obj.PasswordEncSize);
                WriteByteArray(buffer, obj.PasswordEnc);
                WriteByteArray(buffer, new byte[Math.Max(0, 62 - obj.PasswordEnc.Length)]); // u8[62], PC 0x757da0.
            }

            public override S2CCertClientChallengeRes Read(IBuffer buffer)
            {
                S2CCertClientChallengeRes obj = new S2CCertClientChallengeRes();
                ReadServerResponse(buffer, obj);
                obj.PasswordSrcSize = ReadByte(buffer);
                obj.PasswordEncSize = ReadByte(buffer);
                obj.PasswordEnc = ReadByteArray(buffer, 62);
                return obj;
            }
        }
    }
}
