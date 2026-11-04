using System;
using Arrowgene.Ddon.Server.Network;
using Arrowgene.Ddon.Shared.Entity.PacketStructure;
using Arrowgene.Ddon.Shared.Model;
using Arrowgene.Ddon.Shared.Network;
using Xunit;

namespace Arrowgene.Ddon.Test.Server.Network
{
    /// <summary>
    /// Every request gets exactly one response. PC 03040008: a second RES runs the client's handler again (0x878860);
    /// no RES ends in a timeout, dialog 33001 and a disconnect (0x8855a0).
    /// </summary>
    public class RequestHandlerResponseTest
    {
        private static readonly StructurePacket<C2SConnectionPingReq> Request = new StructurePacket<C2SConnectionPingReq>(new C2SConnectionPingReq());

        private class Handler : RequestPacketHandler<RecordingClient, C2SConnectionPingReq, S2CConnectionPingRes>
        {
            private readonly Func<S2CConnectionPingRes> _body;

            public Handler(Func<S2CConnectionPingRes> body) : base(null)
            {
                _body = body;
            }

            public override S2CConnectionPingRes Handle(RecordingClient client, C2SConnectionPingReq request) => _body();
        }

        private class QueueHandler : RequestPacketQueueHandler<RecordingClient, C2SConnectionPingReq, S2CConnectionPingRes>
        {
            private readonly Func<RecordingClient, PacketQueue> _body;

            public QueueHandler(Func<RecordingClient, PacketQueue> body) : base(null)
            {
                _body = body;
            }

            public override PacketQueue Handle(RecordingClient client, C2SConnectionPingReq request) => _body(client);
        }

        private static uint Code(ErrorCode code) => (uint)code;

        [Fact]
        public void A_success_sends_one_response()
        {
            RecordingClient client = new RecordingClient();

            new Handler(() => new S2CConnectionPingRes()).Handle(client, Request);

            Assert.Equal(0u, Assert.Single(client.Frames).Error);
        }

        [Fact]
        public void A_handled_error_sends_one_response()
        {
            RecordingClient client = new RecordingClient();

            new Handler(() => throw new ResponseErrorException(ErrorCode.ERROR_CODE_FAIL)).Handle(client, Request);

            Assert.Equal(Code(ErrorCode.ERROR_CODE_FAIL), Assert.Single(client.Frames).Error);
            Assert.Equal(0, client.Closes);
        }

        [Fact]
        public void A_critical_error_sends_one_response_and_closes()
        {
            RecordingClient client = new RecordingClient();

            new Handler(() => throw new ResponseErrorException(ErrorCode.ERROR_CODE_FAIL, true)).Handle(client, Request);

            Assert.Equal(Code(ErrorCode.ERROR_CODE_FAIL), Assert.Single(client.Frames).Error);
            Assert.Equal(1, client.Closes);
        }

        [Fact]
        public void A_handled_error_in_a_queue_handler_sends_one_response()
        {
            RecordingClient client = new RecordingClient();

            new QueueHandler(_ => throw new ResponseErrorException(ErrorCode.ERROR_CODE_FAIL)).Handle(client, Request);

            Assert.Equal(Code(ErrorCode.ERROR_CODE_FAIL), Assert.Single(client.Frames).Error);
        }
    }
}
