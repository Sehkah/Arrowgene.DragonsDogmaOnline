using Arrowgene.Ddon.Server;
using Arrowgene.Ddon.Shared.Entity.PacketStructure;
using Arrowgene.Ddon.Shared.Network;
using Arrowgene.Logging;

namespace Arrowgene.Ddon.GameServer.Handler
{
    public class QuestLeaderQuestProgressRequestHandler : GameRequestPacketHandler<C2SQuestLeaderQuestProgressRequestReq, S2CQuestLeaderQuestProgressRequestRes>
    {
        private static readonly ServerLogger Logger = LogProvider.Logger<ServerLogger>(typeof(QuestLeaderQuestProgressRequestHandler));
        
        public QuestLeaderQuestProgressRequestHandler(DdonGameServer server) : base(server)
        {
        }

        public override S2CQuestLeaderQuestProgressRequestRes Handle(GameClient client, C2SQuestLeaderQuestProgressRequestReq request)
        {
            client.Party.Leader.Client.Send(new S2CQuestLeaderQuestProgressRequestNtc()
            {
                RequestCharacterId = client.Character.CharacterId,
                QuestScheduleId = request.QuestScheduleId,
                ProcessNo = request.ProcessNo,
                SequenceNo = request.SequenceNo,
                BlockNo = request.BlockNo
            });

            return new S2CQuestLeaderQuestProgressRequestRes()
            {
                QuestProgressResult = 0, // TODO: Proper data
                QuestScheduleId = request.QuestScheduleId,
                ProcessNo = request.ProcessNo
            };
        }
    }
}
