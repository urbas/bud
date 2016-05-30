using System.Collections.Immutable;
using NUnit.Framework;
using static NUnit.Framework.Assert;

namespace Bud.Building {
  public class HasherTest {
    private static readonly byte[] Salt = {0x13};

    [Test]
    public void Md5_produces_a_different_digest_for_different_salts_and_empty_input() {
      AreNotEqual(Hasher.Md5(ImmutableList<string>.Empty, Salt),
                  Hasher.Md5(ImmutableList<string>.Empty, new byte[] {0x37}));
    }

    [Test]
    public void Md5_produces_a_different_digest_for_different_input() {
      using (var dir = new TmpDir()) {
        AreNotEqual(Hasher.Md5(ImmutableList<string>.Empty, Salt),
                    Hasher.Md5(ImmutableList.Create(dir.CreateFile("foo", "a")), Salt));
      }
    }

    [Test]
    public void Md5_produces_the_same_digest_for_same_input() {
      using (var dir = new TmpDir()) {
        var emptyFile = dir.CreateEmptyFile("a");
        AreEqual(Hasher.Md5(ImmutableList.Create(emptyFile), Salt),
                 Hasher.Md5(ImmutableList.Create(emptyFile), Salt));
      }
    }
  }
}