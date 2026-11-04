using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Arrowgene.Ddon.Server.Network;
using Xunit;

namespace Arrowgene.Ddon.Test.Server.Network
{
    public class ChallengeHashTest
    {
        private const int HashOffset = 256 + 16;

        [Fact]
        public void The_hash_of_a_certificate_is_right_on_several_threads()
        {
            // The client checks OpenHash and disconnects on a mismatch (PC 03040008 0x886b5b), and SHA1 instances are not
            // thread safe. Above the default single lane a shared instance could hand out a wrong hash.
            Assert.DoesNotContain(typeof(Challenge).GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static),
                field => typeof(HashAlgorithm).IsAssignableFrom(field.FieldType));

            Challenge[] challenges = Enumerable.Range(0, 4).Select(_ => new Challenge()).ToArray();
            ConcurrentBag<string> wrong = new ConcurrentBag<string>();
            Parallel.ForEach(challenges, new ParallelOptions { MaxDegreeOfParallelism = 4 }, challenge =>
            {
                for (int i = 0; i < 5000; i++)
                {
                    byte[] certificate = challenge.CreateClientCertChallenge();
                    if (!SHA1.HashData(certificate[..HashOffset]).SequenceEqual(certificate[HashOffset..(HashOffset + 20)]))
                    {
                        wrong.Add(Convert.ToHexString(certificate[HashOffset..(HashOffset + 20)]));
                    }
                }
            });

            Assert.Empty(wrong);
        }
    }
}
