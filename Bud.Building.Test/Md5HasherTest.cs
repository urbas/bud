using System.Collections.Immutable;
using NUnit.Framework;
using static NUnit.Framework.Assert;

namespace Bud.Building {
  public class Md5HasherTest {
    private static readonly byte[] Salt = {0x13};

    [Test]
    public void Digest_produces_a_different_digest_for_different_salts_and_empty_input() {
      AreNotEqual(Md5Hasher.Digest(ImmutableList<string>.Empty, Salt),
                  Md5Hasher.Digest(ImmutableList<string>.Empty, new byte[] {0x37}));
    }

    [Test]
    public void Digest_produces_a_different_digest_for_different_input() {
      using (var dir = new TmpDir()) {
        AreNotEqual(Md5Hasher.Digest(ImmutableList<string>.Empty, Salt),
                    Md5Hasher.Digest(dir.CreateFile("foo", "a"), Salt));
      }
    }

    [Test]
    public void Digest_produces_the_same_digest_for_same_input() {
      using (var dir = new TmpDir()) {
        var emptyFile = dir.CreateEmptyFile("a");
        AreEqual(Md5Hasher.Digest(ImmutableList.Create(emptyFile), Salt),
                 Md5Hasher.Digest(emptyFile, Salt));
      }
    }
  }
}