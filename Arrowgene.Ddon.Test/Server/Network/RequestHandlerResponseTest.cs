using System;
using System.Collections.Generic;
using Arrowgene.Buffers;
using Arrowgene.Ddon.Server.Network;
using Arrowgene.Ddon.Shared.Entity;
using Arrowgene.Ddon.Shared.Entity.PacketStructure;
using Arrowgene.Ddon.Shared.Entity.Structure;
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

        [Fact]
        public void Not_implemented_sends_one_response()
        {
            RecordingClient client = new RecordingClient();

            Record.Exception(() => new Handler(() => throw new NotImplementedException()).Handle(client, Request));

            Assert.Equal(Code(ErrorCode.ERROR_CODE_NOT_IMPLEMENTED), Assert.Single(client.Frames).Error);
        }

        [Fact]
        public void Not_implemented_in_a_queue_handler_sends_one_response()
        {
            RecordingClient client = new RecordingClient();

            Record.Exception(() => new QueueHandler(_ => throw new NotImplementedException()).Handle(client, Request));

            Assert.Equal(Code(ErrorCode.ERROR_CODE_NOT_IMPLEMENTED), Assert.Single(client.Frames).Error);
        }

        private class SkillListHandler : RequestPacketHandler<RecordingClient, C2SConnectionPingReq, S2CSkillGetAcquirableSkillListRes>
        {
            private readonly S2CSkillGetAcquirableSkillListRes _response;

            public SkillListHandler(S2CSkillGetAcquirableSkillListRes response) : base(null)
            {
                _response = response;
            }

            public override S2CSkillGetAcquirableSkillListRes Handle(RecordingClient client, C2SConnectionPingReq request) => _response;
        }

        private class SkillListQueueHandler : RequestPacketQueueHandler<RecordingClient, C2SConnectionPingReq, S2CSkillGetAcquirableSkillListRes>
        {
            public SkillListQueueHandler() : base(null)
            {
            }

            public override PacketQueue Handle(RecordingClient client, C2SConnectionPingReq request)
            {
                PacketQueue queue = new PacketQueue();
                client.Enqueue(Oversized(), queue);
                return queue;
            }
        }

        private static S2CSkillGetAcquirableSkillListRes Oversized()
        {
            S2CSkillGetAcquirableSkillListRes response = new S2CSkillGetAcquirableSkillListRes();
            for (int i = 0; i < 70000; i++)
            {
                response.SkillParamList.Add(new CDataSkillParam());
            }

            return response;
        }

        [Fact]
        public void A_response_that_can_not_be_serialized_sends_one_error()
        {
            RecordingClient client = new RecordingClient();

            Exception thrown = Record.Exception(() =>
                new SkillListHandler(new S2CSkillGetAcquirableSkillListRes { SkillParamList = null }).Handle(client, Request));

            Assert.Equal(Code(ErrorCode.ERROR_CODE_FAIL), Assert.Single(client.Frames).Error);
            Assert.NotNull(thrown); // Still reaches the consumer's log and error metric.
        }

        [Fact]
        public void A_response_over_the_frame_limit_sends_one_error()
        {
            RecordingClient client = new RecordingClient();

            Record.Exception(() => new SkillListHandler(Oversized()).Handle(client, Request));

            Assert.Equal(Code(ErrorCode.ERROR_CODE_FAIL), Assert.Single(client.Frames).Error);
        }

        [Fact]
        public void A_queued_response_over_the_frame_limit_sends_one_error()
        {
            RecordingClient client = new RecordingClient();

            Record.Exception(() => new SkillListQueueHandler().Handle(client, Request));

            Assert.Equal(Code(ErrorCode.ERROR_CODE_FAIL), Assert.Single(client.Frames).Error);
        }

        [Fact]
        public void A_failure_after_the_queued_response_left_does_not_send_a_second()
        {
            RecordingClient client = new RecordingClient();
            RecordingClient other = new RecordingClient { ThrowOnSend = true };

            Record.Exception(() => new QueueHandler(c =>
            {
                PacketQueue queue = new PacketQueue();
                c.Enqueue(new S2CConnectionPingRes(), queue);
                other.Enqueue(new S2CConnectionPingRes(), queue);
                return queue;
            }).Handle(client, Request));

            Assert.Equal(0u, Assert.Single(client.Frames).Error);
        }

        /// <summary>
        /// The error path writes a default response: if one of those can not be written, the client gets no RES at all.
        /// </summary>
        [Fact]
        public void Every_registered_response_can_be_written_as_an_error()
        {
            List<string> failed = new List<string>();
            int checkedTypes = 0;
            foreach (Type type in typeof(ServerResponse).Assembly.GetTypes())
            {
                if (type.IsAbstract || !typeof(ServerResponse).IsAssignableFrom(type) || !EntitySerializer.Contains(type))
                {
                    continue;
                }

                checkedTypes++;
                ServerResponse response = (ServerResponse)Activator.CreateInstance(type);
                response.Error = (uint)ErrorCode.ERROR_CODE_FAIL;
                try
                {
                    EntitySerializer serializer = (EntitySerializer)typeof(EntitySerializer)
                        .GetMethod(nameof(EntitySerializer.Get), Type.EmptyTypes)
                        .MakeGenericMethod(type)
                        .Invoke(null, null);
                    serializer.WriteObj(new StreamBuffer(), response);
                }
                catch (Exception ex)
                {
                    failed.Add($"{type.Name}: {ex.GetType().Name}");
                }
            }

            Assert.True(checkedTypes > 500, $"only {checkedTypes} responses found");
            Assert.True(failed.Count == 0, string.Join(Environment.NewLine, failed));
        }
    }
}
