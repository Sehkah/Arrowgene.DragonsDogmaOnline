using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Unicode;
using Arrowgene.Buffers;

namespace Arrowgene.Ddon.Shared
{
    public static class Extension
    {
        public static void WriteMtArray<T>(this IBuffer buffer, List<T> entities, Action<IBuffer, T> writer, Endianness endianness)
        {
            buffer.WriteUInt32((uint) entities.Count, endianness);
            for (int i = 0; i < entities.Count; i++)
            {
                writer.Invoke(buffer, entities[i]);
            }
        }

        public static List<T> ReadMtArray<T>(this IBuffer buffer, Func<IBuffer, T> reader, Endianness endianness)
        {
            List<T> entities = new List<T>();
            uint len = buffer.ReadUInt32(endianness);
            for (int i = 0; i < len; i++)
            {
                entities.Add(reader.Invoke(buffer));
            }

            return entities;
        }
        
        /// <summary>
        /// The most bytes an MtString may have. PC 0x8bc5b0 fails the read above 0x800,
        /// so the cap stays one below that.
        /// </summary>
        public const int MtStringMaxBytes = 0x7FF;

        public static void WriteMtString(this IBuffer buffer, string str)
        {
            if (str == null)
            {
                buffer.WriteUInt16(0, Endianness.Big);
                return;
            }

            byte[] utf8 = Encoding.UTF8.GetBytes(str);
            int length = utf8.Length;
            if (length > MtStringMaxBytes)
            {
                // Stops before a character that does not fit whole.
                Utf8.FromUtf16(str, utf8.AsSpan(0, MtStringMaxBytes), out _, out length);
            }

            buffer.WriteUInt16((ushort) length, Endianness.Big);
            buffer.WriteBytes(utf8, 0, length);
        }

        public static string ReadMtString(this IBuffer buffer)
        {
            ushort len = buffer.ReadUInt16(Endianness.Big);
            string str = buffer.ReadString(len, Encoding.UTF8);
            return str;
        }

        public static bool ReadEnumByte<TEnum>(this IBuffer buffer, out TEnum value) where TEnum : struct
        {
            byte enumByte = buffer.ReadByte();
            value = default;
            if (Enum.IsDefined(typeof(TEnum), enumByte))
            {
                value = (TEnum) (ValueType) enumByte;
                return true;
            }

            return false;
        }

        public static void WriteEnumByte<TEnum>(this IBuffer buffer, TEnum value) where TEnum : struct
        {
            buffer.WriteByte((byte) (ValueType) value);
        }
    }
}
