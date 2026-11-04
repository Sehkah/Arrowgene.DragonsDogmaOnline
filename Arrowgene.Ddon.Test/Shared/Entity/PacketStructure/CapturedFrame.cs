using System.IO;
using Arrowgene.Buffers;
using Arrowgene.Ddon.Shared.Entity;
using Xunit;

namespace Arrowgene.Ddon.Test.Shared.Entity.PacketStructure
{
    /// <summary>
    /// A decrypted frame from a capture of the official server and client 03040008, in TestFiles/Captures. The layout
    /// is as captured. Session keys and tokens are overwritten with X; RSA blocks, identifiers, stale
    /// client bytes and padding with 0xEE.
    /// </summary>
    internal static class CapturedFrame
    {
        private const int HeaderSize = 9;

        public static byte[] Raw(string name) => File.ReadAllBytes(TestUtils.GetTestPath(Path.Combine("Captures", name)));

        public static byte[] Body(string name) => Raw(name)[HeaderSize..];

        /// <summary>
        /// The reader takes exactly the bytes the client's layout has, and the writer gives the same bytes back.
        /// </summary>
        public static T AssertRoundTrip<T>(byte[] body, int layoutLength) where T : class, new()
        {
            EntitySerializer<T> serializer = EntitySerializer.Get<T>();
            IBuffer buffer = new StreamBuffer(body);
            buffer.SetPositionStart();

            T read = serializer.Read(buffer);

            Assert.Equal(layoutLength, buffer.Position);
            Assert.Equal(body[..layoutLength], serializer.Write(read));
            return read;
        }
    }
}
