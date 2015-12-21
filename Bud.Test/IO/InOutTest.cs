using System.Collections.Immutable;
using NUnit.Framework;

namespace Bud.IO {
  public class InOutTest {
    private readonly IInOut fileAOkay = InOutFile.ToInOutFile("a");
    private readonly IInOut fileBOkay = InOutFile.ToInOutFile("b");
    private InOut inOutSingleOkayFileA;
    private InOut inOutSingleOkayFileB;

    [SetUp]
    public void SetUp() {
      inOutSingleOkayFileA = new InOut(ImmutableList.Create(fileAOkay));
      inOutSingleOkayFileB = new InOut(ImmutableList.Create(fileBOkay));
    }

    [Test]
    public void Empty_contains_no_files() => Assert.IsEmpty(InOut.Empty.Elements);

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
                         new InOut(ImmutableList<IInOut>.Empty));

    [Test]
    public void Empties_are_not_same()
      => Assert.AreNotSame(InOut.Empty,
                           new InOut(ImmutableList<IInOut>.Empty));

    [Test]
    public void Hash_code_equals_when_empty()
      => Assert.AreEqual(InOut.Empty.GetHashCode(),
                         new InOut(ImmutableList<IInOut>.Empty).GetHashCode());

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