using System.Linq;
using System.Text;
using Arrowgene.Ddon.Shared.Entity;
using Arrowgene.Ddon.Shared.Entity.PacketStructure;
using Xunit;

namespace Arrowgene.Ddon.Test.Shared.Entity
{
    /// <summary>
    /// Through a real structure, so the serializer's writer is what is tested: its body is {u32, u32, MtString, ...}.
    /// </summary>
    public class MtStringWriteTest
    {
        private const int LengthOffset = 8;
        private const int TextOffset = 10;

        private static byte[] WrittenText(string text)
        {
            byte[] written = EntitySerializer.Get<L2CGetGameSessionKeyRes>().Write(new L2CGetGameSessionKeyRes { SessionKey = text });
            int length = (written[LengthOffset] << 8) | written[LengthOffset + 1];
            return written[TextOffset..(TextOffset + length)];
        }

        [Theory]
        [InlineData(0x7FE)]
        [InlineData(0x7FF)]
        public void A_string_under_the_cap_is_unchanged(int length)
        {
            string text = new string('a', length);

            Assert.Equal(Encoding.UTF8.GetBytes(text), WrittenText(text));
        }

        [Theory]
        [InlineData(0x800)]
        [InlineData(0x801)]
        [InlineData(0x10000)]
        public void A_longer_string_stops_below_the_client_limit(int length)
        {
            Assert.Equal(0x7FF, WrittenText(new string('a', length)).Length);
        }

        [Theory]
        [InlineData("\u3042", 0x7FF)] // Three bytes each: 0x7FF is a boundary.
        [InlineData("a\u3042", 0x7FF)] // 1 + 3n: 0x7FF is not, so it stops earlier.
        [InlineData("\U0001F600", 0x7FC)] // Four bytes, two chars: a surrogate pair stays whole.
        public void Truncation_keeps_whole_characters(string unit, int most)
        {
            string text = string.Concat(Enumerable.Repeat(unit, 0x1000));

            byte[] written = WrittenText(text);

            Assert.True(written.Length <= most, $"{written.Length} bytes");
            Assert.True(written.Length > most - 4, $"{written.Length} bytes");
            string decoded = new UTF8Encoding(false, true).GetString(written);
            Assert.StartsWith(decoded, text);
        }

        [Fact]
        public void Null_writes_length_zero()
        {
            Assert.Empty(WrittenText(null));
        }
    }
}
