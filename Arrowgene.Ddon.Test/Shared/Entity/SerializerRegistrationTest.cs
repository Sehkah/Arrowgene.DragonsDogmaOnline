using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Arrowgene.Buffers;
using Arrowgene.Ddon.Shared.Entity;
using Arrowgene.Ddon.Shared.Entity.PacketStructure;
using Arrowgene.Ddon.Shared.Entity.Structure;
using Xunit;

namespace Arrowgene.Ddon.Test.Shared.Entity
{
    public class SerializerRegistrationTest
    {
        private const BindingFlags Declared = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic
                                              | BindingFlags.Instance | BindingFlags.Static;

        /// <summary>Methods of EntitySerializer that look an entity up by its type parameter.</summary>
        private static readonly HashSet<string> EntityLookups = new HashSet<string>
        {
            "Get", "WriteEntity", "ReadEntity", "WriteEntityList", "ReadEntityList"
        };

        private static readonly Dictionary<short, OpCode> OpCodesByValue = typeof(OpCodes)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(field => (OpCode)field.GetValue(null))
            .ToDictionary(code => code.Value);

        /// <summary>
        /// Reflection over the serializers' own code: every entity a registered serializer looks up by type has to be
        /// registered too, or a list of it writes no elements and reads no bytes. The IL decode is own code:
        /// the BCL has no reader for method bodies, and Mono.Cecil is not worth a dependency for one test.
        /// </summary>
        [Fact]
        public void Every_entity_reached_by_a_registered_serializer_is_registered()
        {
            List<(Type Serializer, Type Entity)> edges = new List<(Type, Type)>();
            foreach (Type serializer in typeof(EntitySerializer).Assembly.GetTypes().Where(IsRegisteredSerializer))
            {
                foreach (Type entity in EntitiesLookedUpBy(serializer))
                {
                    edges.Add((serializer, entity));
                }
            }

            // The walk has to have seen what it is here for, or a change to the IL shape would pass this test silently.
            Assert.True(edges.Count > 1000, $"only {edges.Count} lookups found");
            Assert.Contains(edges, edge => edge.Serializer == typeof(CDataContextSetAdditional.Serializer) && edge.Entity == typeof(CDataOcdImmuneLv));
            Assert.Contains(edges, edge => edge.Serializer == typeof(S2CItemSwitchStorageNtc.Serializer) && edge.Entity == typeof(CDataSwitchStorage));

            List<string> missing = edges
                .Where(edge => !edge.Entity.IsGenericParameter && !EntitySerializer.Contains(edge.Entity))
                .Select(edge => $"{edge.Serializer.FullName} -> {edge.Entity.FullName}")
                .Distinct()
                .ToList();
            Assert.True(missing.Count == 0, string.Join(Environment.NewLine, missing));
        }

        private static bool IsRegisteredSerializer(Type type)
        {
            if (type.IsAbstract || type.ContainsGenericParameters || !typeof(EntitySerializer).IsAssignableFrom(type))
            {
                return false;
            }

            Type baseType = type;
            while (baseType != null && !(baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(EntitySerializer<>)))
            {
                baseType = baseType.BaseType;
            }

            return baseType != null && EntitySerializer.Contains(baseType.GetGenericArguments()[0]);
        }

        private static IEnumerable<Type> EntitiesLookedUpBy(Type serializer)
        {
            // Nested types too: the compiler puts lambdas there.
            IEnumerable<Type> scopes = new[] { serializer }.Concat(serializer.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic));
            foreach (MethodBase method in scopes.SelectMany(scope => scope.GetMethods(Declared).Cast<MethodBase>().Concat(scope.GetConstructors(Declared))))
            {
                foreach (MethodBase called in CallsIn(method))
                {
                    if (called.IsGenericMethod && EntityLookups.Contains(called.Name) && typeof(EntitySerializer).IsAssignableFrom(called.DeclaringType))
                    {
                        yield return called.GetGenericArguments()[0];
                    }
                }
            }
        }

        private static IEnumerable<MethodBase> CallsIn(MethodBase method)
        {
            byte[] il = method.GetMethodBody()?.GetILAsByteArray();
            if (il == null)
            {
                yield break;
            }

            int position = 0;
            while (position < il.Length)
            {
                short value = il[position++];
                if (value == 0xFE)
                {
                    value = unchecked((short)(0xFE00 | il[position++]));
                }

                OpCode code = OpCodesByValue[value];
                switch (code.OperandType)
                {
                    case OperandType.InlineMethod:
                        yield return method.Module.ResolveMethod(BitConverter.ToInt32(il, position),
                            method.DeclaringType.IsGenericType ? method.DeclaringType.GetGenericArguments() : null,
                            method.IsGenericMethod ? method.GetGenericArguments() : null);
                        position += 4;
                        break;
                    case OperandType.InlineSwitch:
                        position += 4 + 4 * BitConverter.ToInt32(il, position);
                        break;
                    case OperandType.InlineI8:
                    case OperandType.InlineR:
                        position += 8;
                        break;
                    case OperandType.InlineNone:
                        break;
                    case OperandType.InlineVar:
                        position += 2;
                        break;
                    case OperandType.ShortInlineBrTarget:
                    case OperandType.ShortInlineI:
                    case OperandType.ShortInlineVar:
                        position += 1;
                        break;
                    default:
                        position += 4;
                        break;
                }
            }
        }

        private static byte[] WriteContext(C2SContextSetContextNtc value)
        {
            IBuffer buffer = new StreamBuffer();
            EntitySerializer.Get<C2SContextSetContextNtc>().Write(buffer, value);
            return buffer.GetAllBytes();
        }

        /// <summary>The offset of the immune list's u32 count in a 35.9.16 body.</summary>
        private static int ImmuneCountOffset()
        {
            C2SContextSetContextNtc one = new C2SContextSetContextNtc();
            one.Additional.ImmuneLvList.Add(new CDataOcdImmuneLv());
            byte[] counted = WriteContext(one);
            byte[] zero = WriteContext(new C2SContextSetContextNtc());
            return Enumerable.Range(0, zero.Length).First(i => zero[i] != counted[i]) - 3;
        }

        [Fact]
        public void A_list_of_ocd_immune_levels_consumes_its_elements()
        {
            Assert.True(EntitySerializer.Contains(typeof(CDataOcdImmuneLv)), "CDataOcdImmuneLv is not registered");

            byte[] zero = WriteContext(new C2SContextSetContextNtc());
            int at = ImmuneCountOffset();
            byte[] body = new byte[zero.Length + 4];
            Array.Copy(zero, body, at);
            body[at + 3] = 2;
            // 2 elements of {u8, u8} per PC 03040008 0x76fc20, then the rest of the empty packet.
            Array.Copy(new byte[] { 1, 2, 3, 4 }, 0, body, at + 4, 4);
            Array.Copy(zero, at + 4, body, at + 8, zero.Length - at - 4);

            IBuffer buffer = new StreamBuffer(body);
            buffer.SetPositionStart();
            C2SContextSetContextNtc read = EntitySerializer.Get<C2SContextSetContextNtc>().Read(buffer);

            Assert.Equal(2, read.Additional.ImmuneLvList.Count);
            Assert.Equal(4, read.Additional.ImmuneLvList[1].ImmuneLv);
            Assert.Equal(body.Length, buffer.Position);
        }

        [Fact]
        public void A_huge_list_count_ends_at_the_end_of_the_data()
        {
            // First, since the read below relies on the element being registered.
            Assert.True(EntitySerializer.Contains(typeof(CDataOcdImmuneLv)), "CDataOcdImmuneLv is not registered");

            byte[] body = WriteContext(new C2SContextSetContextNtc());
            int at = ImmuneCountOffset();
            body[at] = body[at + 1] = body[at + 2] = body[at + 3] = 0xFF;

            Assert.Throws<EndOfStreamException>(() => EntitySerializer.Get<C2SContextSetContextNtc>().Read(body));
        }

        [Fact]
        public void A_captured_switch_storage_notice_reads_its_element()
        {
            // 10.13.16: Unk0, IsStart, count 1, {StorageType 1, Num 10}. The
            // padding after it is left out: the official server does not zero it.
            byte[] body = { 0, 0, 0, 0, 1, 0, 0, 0, 1, 1, 0, 0x0A };

            IBuffer buffer = new StreamBuffer(body);
            buffer.SetPositionStart();
            S2CItemSwitchStorageNtc read = EntitySerializer.Get<S2CItemSwitchStorageNtc>().Read(buffer);

            CDataSwitchStorage element = Assert.Single(read.ChangeList);
            Assert.NotNull(element);
            Assert.Equal((ushort)10, element.Num);
            Assert.Equal(body.Length, buffer.Position);
        }

        private class Unregistered
        {
        }

        private class SerializerOfUnregistered : EntitySerializer<Unregistered>
        {
            public override void Write(IBuffer buffer, Unregistered obj)
            {
                WriteEntityList(buffer, new List<Unregistered> { obj });
            }

            public override Unregistered Read(IBuffer buffer)
            {
                return ReadEntity<Unregistered>(buffer);
            }
        }

        [Fact]
        public void An_entity_with_no_serializer_fails_loudly()
        {
            SerializerOfUnregistered serializer = new SerializerOfUnregistered();

            Assert.Throws<InvalidOperationException>(() => serializer.Write(new Unregistered()));
            Assert.Throws<InvalidOperationException>(() => serializer.Read(new byte[4]));
        }
    }
}
