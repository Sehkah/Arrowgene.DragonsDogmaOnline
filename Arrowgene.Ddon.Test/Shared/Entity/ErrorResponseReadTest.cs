using System;
using System.Collections.Generic;
using System.IO;
using Arrowgene.Buffers;
using Arrowgene.Ddon.Shared.Entity;
using Arrowgene.Ddon.Shared.Entity.PacketStructure;
using Arrowgene.Ddon.Shared.Network;
using Arrowgene.Ddon.Test.Shared.Entity.PacketStructure;
using Xunit;

namespace Arrowgene.Ddon.Test.Shared.Entity
{
    public class ErrorResponseReadTest
    {
        // One capture of each of the 4 ids that carry Error != 0, chosen as the smallest:
        // 13.3.2, 18.0.2, 19.1.2 and 19.2.2.
        [Fact]
        public void A_captured_kill_error_stops_at_the_error()
        {
            AssertStops<S2CInstanceEnemyKillRes>("13_3_2.bin", 16016);
        }

        [Fact]
        public void A_captured_member_list_error_stops_at_the_error()
        {
            AssertStops<S2CGroupChatGetMemberListRes>("18_0_2.bin", 47003);
        }

        [Fact]
        public void A_captured_skill_list_error_stops_at_the_error()
        {
            AssertStops<S2CSkillGetAcquirableSkillListRes>("19_1_2.bin", 15005);
        }

        [Fact]
        public void A_captured_ability_list_error_stops_at_the_error()
        {
            AssertStops<S2CSkillGetAcquirableAbilityListRes>("19_2_2.bin", 15005);
        }

        private static void AssertStops<T>(string capture, uint error) where T : ServerResponse, new()
        {
            byte[] body = CapturedFrame.Body(capture);
            EntitySerializer<T> serializer = EntitySerializer.Get<T>();

            IBuffer buffer = new StreamBuffer(body);
            buffer.SetPositionStart();
            T read = (T)serializer.ReadObj(buffer);

            Assert.Equal(error, read.Error);
            Assert.Equal(0u, read.Result);
            Assert.Equal(4, buffer.Position);
            Assert.Equal(error, serializer.Read(body).Error);

            T structure = new T();
            StructurePacket<T> packet = new StructurePacket<T>(new Packet(structure.Id, body));
            Assert.Equal(error, packet.Structure.Error);
        }

        [Fact]
        public void A_response_without_an_error_reads_on()
        {
            // 62.33.2 and the rest of the captured responses are the wider test of this; this one needs no capture.
            byte[] body = { 0, 0, 0, 0, 0, 0, 0, 7 };

            Assert.Equal(7u, EntitySerializer.Get<S2CClanClanSettingUpdateRes>().Read(body).Result);
        }

        [Fact]
        public void Every_registered_response_stops_at_an_error()
        {
            byte[] body = { 0, 0, 0x3E, 0x90, 0, 0, 0 }; // An Error and 3 bytes of padding: the shortest body captured.
            List<string> failed = new List<string>();
            int checkedTypes = 0;
            foreach (Type type in typeof(ServerResponse).Assembly.GetTypes())
            {
                if (type.IsAbstract || !typeof(ServerResponse).IsAssignableFrom(type) || !EntitySerializer.Contains(type))
                {
                    continue;
                }

                EntitySerializer serializer = (EntitySerializer)typeof(EntitySerializer)
                    .GetMethod(nameof(EntitySerializer.Get), Type.EmptyTypes)
                    .MakeGenericMethod(type)
                    .Invoke(null, null);

                // Some notices derive from ServerResponse and have no RES header: skip what does not write one.
                ServerResponse response = (ServerResponse)Activator.CreateInstance(type);
                response.Error = 0x3E90;
                IBuffer written = new StreamBuffer();
                serializer.WriteObj(written, response);
                if (written.Size < 8 || !written.GetAllBytes()[..4].AsSpan().SequenceEqual(body.AsSpan(0, 4)))
                {
                    continue;
                }

                checkedTypes++;
                IBuffer buffer = new StreamBuffer(body);
                buffer.SetPositionStart();
                try
                {
                    ServerResponse read = (ServerResponse)serializer.ReadObj(buffer);
                    if (read.Error != 0x3E90 || buffer.Position != 4)
                    {
                        failed.Add($"{type.Name}: Error {read.Error}, position {buffer.Position}");
                    }
                }
                catch (Exception ex) when (ex is EndOfStreamException or InvalidOperationException)
                {
                    failed.Add($"{type.Name}: {ex.GetType().Name}");
                }
            }

            Assert.True(checkedTypes > 480, $"only {checkedTypes} responses found");
            Assert.True(failed.Count == 0, $"{failed.Count} of {checkedTypes}: {string.Join(", ", failed)}");
        }
    }
}
