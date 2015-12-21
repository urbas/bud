using System.Collections.Immutable;
using NUnit.Framework;

namespace Bud.IO {
  public class InOutTest {
    private readonly string fileA = "a";
    private readonly string fileB = "b";
    private InOut inOutSingleOkayFileA;
    private InOut inOutSingleOkayFileB;

    [SetUp]
    public void SetUp() {
      inOutSingleOkayFileA = new InOut(ImmutableList.Create(fileA));
      inOutSingleOkayFileB = new InOut(ImmutableList.Create(fileB));
    }

    [Test]
    public void Empty_contains_no_files() => Assert.IsEmpty(InOut.Empty.Elements);

    [Test]
    public void Merging_empty_is_empty()
      => Assert.AreEqual(InOut.Empty, InOut.Merge(InOut.Empty, InOut.Empty));

    [Test]
    public void Merging_non_empty()
      => Assert.AreEqual(new InOut(ImmutableList.Create(fileA, fileB)),
                         InOut.Merge(inOutSingleOkayFileA, inOutSingleOkayFileB));

    [Test]
    public void Empties_equal()
      => Assert.AreEqual(InOut.Empty,
                         new InOut(ImmutableList<object>.Empty));

    [Test]
    public void Empties_are_not_same()
      => Assert.AreNotSame(InOut.Empty,
                           new InOut(ImmutableList<object>.Empty));

    [Test]
    public void Hash_code_equals_when_empty()
      => Assert.AreEqual(InOut.Empty.GetHashCode(),
                         new InOut(ImmutableList<object>.Empty).GetHashCode());

    [Test]
    public void Equals_when_files_are_the_same()
      => Assert.AreEqual(inOutSingleOkayFileA,
                         new InOut(ImmutableList.Create(fileA)));

    [Test]
    public void Hash_code_equals_when_instances_equal()
      => Assert.AreEqual(inOutSingleOkayFileA.GetHashCode(),
                         new InOut(ImmutableList.Create(fileA)).GetHashCode());
  }
}