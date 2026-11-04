using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Arrowgene.Ddon.LoginServer;
using Arrowgene.Ddon.LoginServer.Handler;
using Arrowgene.Ddon.Shared;
using Arrowgene.Ddon.Shared.Entity;
using Arrowgene.Ddon.Shared.Entity.PacketStructure;
using Arrowgene.Ddon.Shared.Model;
using Xunit;

namespace Arrowgene.Ddon.Test.Server.Network
{
    public class ErrorMessageListHandlerTest
    {
        private static void SetAutoProperty(object target, string name, object value)
        {
            for (Type type = target.GetType(); type != null; type = type.BaseType)
            {
                FieldInfo field = type.GetField($"<{name}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
                if (field != null)
                {
                    field.SetValue(target, value);
                    return;
                }
            }

            throw new InvalidOperationException(name);
        }

        [Fact]
        public void Each_frame_carries_the_count_still_to_come()
        {
            // Long messages, so the handler has to split them over frames.
            Dictionary<ErrorCode, ClientErrorCode> messages = Enum.GetValues<ErrorCode>().Distinct().ToDictionary(code => code, code => new ClientErrorCode
            {
                Code = code,
                Message = new Dictionary<string, string> { { "en", new string('m', 500) } },
            });
            AssetRepository assets = (AssetRepository)RuntimeHelpers.GetUninitializedObject(typeof(AssetRepository));
            SetAutoProperty(assets, nameof(AssetRepository.ClientErrorCodes), messages);
            DdonLoginServer server = (DdonLoginServer)RuntimeHelpers.GetUninitializedObject(typeof(DdonLoginServer));
            SetAutoProperty(server, nameof(DdonLoginServer.AssetRepository), assets);
            GetErrorMessageListHandler handler = (GetErrorMessageListHandler)RuntimeHelpers.GetUninitializedObject(typeof(GetErrorMessageListHandler));
            SetAutoProperty(handler, "Server", server);
            RecordingLoginClient client = new RecordingLoginClient();

            handler.Handle(client, new C2LGetErrorMessageListReq());

            Assert.True(client.Frames.Count > 1, $"{client.Frames.Count} frames");
            int total = Enum.GetValues<ErrorCode>().Length;
            int sent = 0;
            foreach (SentFrame frame in client.Frames)
            {
                L2CGetErrorMessageListNtc ntc = EntitySerializer.Get<L2CGetErrorMessageListNtc>().Read(frame.Plain[9..]);
                sent += ntc.ErrorMessages.Count;
                Assert.Equal((uint)(total - sent), ntc.RemainingNum);
            }

            Assert.Equal(total, sent);
        }
    }
}
