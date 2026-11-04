using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Arrowgene.Ddon.Shared.Network;
using Xunit;

namespace Arrowgene.Ddon.Test.Shared.Network
{
    public class PacketIdTest
    {
        private static IEnumerable<PacketId> Declared(ServerType serverType) => typeof(PacketId)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.FieldType == typeof(PacketId))
            .Select(field => (PacketId)field.GetValue(null))
            .Where(id => id.ServerType == serverType && id != PacketId.UNKNOWN);

        /// <summary>
        /// HashCode.Combine is seeded per process, so a collision among the 1,778 game ids could stop a start with a
        /// duplicate key, and an unknown id could resolve to the wrong packet, at about 3.7e-4 per start. By chance a test
        /// can not see that, so this one states the key.
        /// </summary>
        [Theory]
        [InlineData("LoginPacketIds")]
        [InlineData("GamePacketIds")]
        public void The_lookup_is_keyed_by_group_id_and_sub_not_by_a_hash(string field)
        {
            Type type = typeof(PacketId).GetField(field, BindingFlags.NonPublic | BindingFlags.Static).FieldType;

            Assert.Equal(typeof(ValueTuple<byte, ushort, byte>), type.GetGenericArguments()[0]);
        }

        [Fact]
        public void Every_declared_id_resolves_to_itself_and_none_shares_a_key()
        {
            foreach (PacketId id in Declared(ServerType.Login))
            {
                Assert.Equal(id.Name, PacketId.GetLoginPacketId(id.GroupId, id.HandlerId, id.HandlerSubId).Name);
            }

            foreach (PacketId id in Declared(ServerType.Game))
            {
                Assert.Equal(id.Name, PacketId.GetGamePacketId(id.GroupId, id.HandlerId, id.HandlerSubId).Name);
            }

            Assert.True(Declared(ServerType.Game).Count() > 1700);
        }

        [Fact]
        public void An_unknown_id_resolves_to_unknown()
        {
            Assert.Equal("Unknown", PacketId.GetGamePacketId(255, 65535, 254).Name);
            Assert.Equal("Unknown", PacketId.GetLoginPacketId(255, 65535, 254).Name);
        }
    }
}
