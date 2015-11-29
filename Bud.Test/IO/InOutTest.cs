using System.Collections.Immutable;
using NUnit.Framework;

namespace Bud.IO {
  public class InOutTest {
    private readonly InOutFile fileAOkay = InOutFile.Create("a");
    private readonly InOutFile fileBOkay = InOutFile.Create("b");
    private readonly InOutFile fileANotOkay = InOutFile.Create("a", false);
    private InOut inOutSingleOkayFileA;
    private InOut inOutSingleOkayFileB;

    [SetUp]
    public void SetUp() {
      inOutSingleOkayFileA = new InOut(ImmutableList.Create(fileAOkay));
      inOutSingleOkayFileB = new InOut(ImmutableList.Create(fileBOkay));
    }

    [Test]
    public void Empty_contains_no_files() => Assert.IsEmpty(InOut.Empty.Files);

    [Test]
    public void Empty_is_okay() => Assert.IsTrue(InOut.Empty.IsOkay);

    [Test]
    public void Is_not_okay_when_some_files_are_not_okay()
      => Assert.IsFalse(new InOut(ImmutableList.Create(fileANotOkay)).IsOkay);

    [Test]
    public void Is_not_okay_when_all_files_are_okay()
      => Assert.IsTrue(inOutSingleOkayFileA.IsOkay);

    [Test]
    public void Adding_files()
      => Assert.That(InOut.Empty.AddFiles("a").Files,
                     Is.EquivalentTo(new[] {fileAOkay}));

    [Test]
    public void Merging_empty_is_empty()
      => Assert.AreEqual(InOut.Empty, InOut.Merge(InOut.Empty, InOut.Empty));

    [Test]
    public void Merging_non_empty()
      => Assert.AreEqual(new InOut(ImmutableList.Create(fileAOkay, fileBOkay)),
                         InOut.Merge(inOutSingleOkayFileA, inOutSingleOkayFileB));

    [Test]
    public void Empties_equal()
      => Assert.AreEqual(InOut.Empty,
                         new InOut(ImmutableList<InOutFile>.Empty));

    [Test]
    public void Empties_are_not_same()
      => Assert.AreNotSame(InOut.Empty,
                           new InOut(ImmutableList<InOutFile>.Empty));

    [Test]
    public void Hash_code_equals_when_empty()
      => Assert.AreEqual(InOut.Empty.GetHashCode(),
                         new InOut(ImmutableList<InOutFile>.Empty).GetHashCode());

    [Test]
    public void Equals_when_files_are_the_same()
      => Assert.AreEqual(inOutSingleOkayFileA,
                         new InOut(ImmutableList.Create(fileAOkay)));

    [Test]
    public void Hash_code_equals_when_instances_equal()
      => Assert.AreEqual(inOutSingleOkayFileA.GetHashCode(),
                         new InOut(ImmutableList.Create(fileAOkay)).GetHashCode());
  }
}