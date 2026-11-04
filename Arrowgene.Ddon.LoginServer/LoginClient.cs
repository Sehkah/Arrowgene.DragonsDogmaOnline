using Arrowgene.Ddon.Database.Model;
using Arrowgene.Ddon.Server.Network;
using Arrowgene.Networking.SAEAServer;

namespace Arrowgene.Ddon.LoginServer
{
    public class LoginClient : Client
    {
        public LoginClient(ClientHandle clientHandle, PacketFactory packetFactory) : base(clientHandle, packetFactory)
        {
            UpdateIdentity();
        }

        /// <summary>
        /// A client with no socket behind it. See <see cref="Client(PacketFactory, string)"/>.
        /// </summary>
        protected LoginClient(PacketFactory packetFactory, string identity) : base(packetFactory, identity)
        {
        }

        public void UpdateIdentity()
        {
            string newIdentity = $"[LoginClient#{Id}@{ClientHandle.Identity}]";
            if (Account != null)
            {
                newIdentity += $"[Acc:{Account.NormalName}]";
            }

            Identity = newIdentity;
        }

        public Account Account { get; set; }

        public uint SelectedCharacterId { get; set; }
    }
}
