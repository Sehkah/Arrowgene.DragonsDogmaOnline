using System;
using System.Runtime.CompilerServices;
using Arrowgene.Ddon.GameServer.Handler;
using Arrowgene.Ddon.LoginServer.Handler;
using Arrowgene.Ddon.Server.Network;
using Arrowgene.Ddon.Shared.Entity.PacketStructure;
using Arrowgene.Ddon.Shared.Model;
using Arrowgene.Ddon.Shared.Network;
using Xunit;

namespace Arrowgene.Ddon.Test.Server.Network
{
    /// <summary>
    /// The 7 REQ handlers on a non-request base: every path sends a RES, as no RES ends the session (PC 03040008 0x8855a0).
    /// </summary>
    public class RequestOnPlainHandlerTest
    {
        private static void AssertAnswers<TClient>(PacketHandler<TClient> handler, TClient client, IPacket request, PacketId req)
            where TClient : Arrowgene.Ddon.Server.Network.Client
        {
            Record.Exception(() => handler.Handle(client, request));

            SentFrame frame = Assert.Single(((IRecordingClient)client).Frames);
            Assert.Equal(req.GroupId, frame.GroupId);
            Assert.Equal(req.HandlerId, frame.HandlerId);
            Assert.Equal((byte)2, frame.HandlerSubId);
        }

        [Fact]
        public void Quest_leader_progress_with_no_party_answers_with_an_error()
        {
            RecordingGameClient client = new RecordingGameClient();
            var request = new StructurePacket<C2SQuestLeaderQuestProgressRequestReq>(new C2SQuestLeaderQuestProgressRequestReq());

            AssertAnswers(new QuestLeaderQuestProgressRequestHandler(null), client, request, request.Id);

            Assert.Equal((uint)ErrorCode.ERROR_CODE_FAIL, client.Frames[0].Error);
        }

        [Fact]
        public void Create_character_that_fails_answers_with_an_error()
        {
            RecordingLoginClient client = new RecordingLoginClient();
            byte[] body = System.IO.File.ReadAllBytes(TestUtils.GetTestPath("C2LCreateCharacterDataReqTestData.bin"));

            // The constructor reads the asset repository from the server, and client.Account is null before any of it is used.
            var handler = (CreateCharacterHandler)RuntimeHelpers.GetUninitializedObject(typeof(CreateCharacterHandler));
            AssertAnswers(handler, client, new Packet(PacketId.C2L_CREATE_CHARACTER_DATA_REQ, body), PacketId.C2L_CREATE_CHARACTER_DATA_REQ);

            Assert.Equal((uint)ErrorCode.ERROR_CODE_FAIL, client.Frames[0].Error);
        }

        // These four send first, or send only a canned dump, so they stay on their base: the tests pin that.

        [Fact]
        public void Lobby_leave_answers_first()
        {
            RecordingGameClient client = new RecordingGameClient();
            var request = new StructurePacket<C2SLobbyLeaveReq>(new C2SLobbyLeaveReq());
            var handler = (LobbyLobbyLeaveHandler)RuntimeHelpers.GetUninitializedObject(typeof(LobbyLobbyLeaveHandler));

            AssertAnswers(handler, client, request, request.Id);
        }

        [Fact]
        public void Quest_play_entry_answers_first()
        {
            RecordingGameClient client = new RecordingGameClient();
            var request = new StructurePacket<C2SQuestPlayEntryReq>(new C2SQuestPlayEntryReq());

            AssertAnswers(new QuestPlayEntryHandler(null), client, request, request.Id);
        }

        [Fact]
        public void Canned_dumps_answer_with_their_own_response()
        {
            AssertAnswers(new DailyMissionListGetHandler(null), new RecordingGameClient(), new Packet(PacketId.C2S_DAILY_MISSION_LIST_GET_REQ, Array.Empty<byte>()), PacketId.C2S_DAILY_MISSION_LIST_GET_REQ);
            AssertAnswers(new FriendGetRecentCharacterListHandler(null), new RecordingGameClient(), new Packet(PacketId.C2S_FRIEND_GET_RECENT_CHARACTER_LIST_REQ, Array.Empty<byte>()), PacketId.C2S_FRIEND_GET_RECENT_CHARACTER_LIST_REQ);
            AssertAnswers(new GpGetUpdateAppCourseBonusFlagHandler(null), new RecordingGameClient(), new Packet(PacketId.C2S_GP_GET_UPDATE_APP_COURSE_BONUS_FLAG_REQ, Array.Empty<byte>()), PacketId.C2S_GP_GET_UPDATE_APP_COURSE_BONUS_FLAG_REQ);
        }
    }
}
