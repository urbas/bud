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
                    Md5Hasher.Digest(ImmutableList.Create(dir.CreateFile("foo", "a")), Salt));
      }
    }

    [Test]
    public void Digest_produces_the_same_digest_for_same_input() {
      using (var dir = new TmpDir()) {
        var file = dir.CreateFile("foo bar", "a");
        AreEqual(Md5Hasher.Digest(ImmutableList.Create(file), Salt),
                 Md5Hasher.Digest(ImmutableList.Create(file), Salt));
      }
    }

    [Test]
    public void Digest_produces_different_digest_for_different_order_of_input() {
      using (var dir = new TmpDir()) {
        var fileA = dir.CreateFile("foo bar", "a");
        var fileB = dir.CreateFile("1337", "b");
        AreNotEqual(Md5Hasher.Digest(ImmutableList.Create(fileA, fileB), Salt),
                    Md5Hasher.Digest(ImmutableList.Create(fileB, fileA), Salt));
      }
    }
  }
}