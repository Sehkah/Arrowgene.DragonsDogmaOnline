using Arrowgene.Ddon.Shared.Entity.PacketStructure;
using Arrowgene.Ddon.Shared.Network;
using Xunit;

namespace Arrowgene.Ddon.Test.Server.Network
{
    public class ClientSeamTest
    {
        [Fact]
        public void A_client_without_a_socket_records_what_it_sends()
        {
            RecordingClient client = new RecordingClient();

            client.Send(new S2CConnectionPingRes());
            client.Close();

            SentFrame frame = Assert.Single(client.Frames);
            Assert.Equal(PacketId.S2C_CONNECTION_PING_RES.GroupId, frame.GroupId);
            Assert.Equal(PacketId.S2C_CONNECTION_PING_RES.HandlerId, frame.HandlerId);
            Assert.Equal(PacketId.S2C_CONNECTION_PING_RES.HandlerSubId, frame.HandlerSubId);
            Assert.Equal(1, client.Closes);
        }

        [Fact]
        public void A_game_and_a_login_client_exist_without_a_socket()
        {
            RecordingGameClient game = new RecordingGameClient();
            RecordingLoginClient login = new RecordingLoginClient();

            game.Send(new S2CConnectionPingRes());
            login.Send(new L2CPingRes());

            Assert.Single(game.Frames);
            Assert.Single(login.Frames);
        }
    }
}
