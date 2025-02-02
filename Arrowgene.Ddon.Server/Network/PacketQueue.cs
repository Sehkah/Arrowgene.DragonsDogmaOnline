using Arrowgene.Ddon.Shared.Entity;
using Arrowgene.Ddon.Shared.Network;
using System.Collections.Generic;

namespace Arrowgene.Ddon.Server.Network
{
    public class PacketQueue : Queue<(Client Client, Packet Packet)>
    {
        public PacketQueue() { }
        public void Send()
        {
            foreach ((Client Client, Packet Packet) in this)
            {
                Client.Send(Packet);
            }
        }

        public void Enqueue<TResStruct>(Client client, TResStruct res)
            where TResStruct : class, IPacketStructure, new()
        {
            StructurePacket<TResStruct> packet = new StructurePacket<TResStruct>(res);
            this.Enqueue((client, packet));
        }

        public void AddRange(IEnumerable<(Client Client, Packet Packet)> other)
        {
            foreach(var item in other)
            {
                this.Enqueue(item);
            }
        }
    }
}
