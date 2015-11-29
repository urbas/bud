using NUnit.Framework;

namespace Bud.IO {
  public class InOutFileTest {
    private readonly InOutFile filaAOkay = InOutFile.Create("a");

    [Test]
    public void Path_is_stored() => Assert.AreEqual("a", filaAOkay.Path);

    [Test]
    public void IsOkay_is_stored() => Assert.IsTrue(filaAOkay.IsOkay);

    [Test]
    public void Files_equal()
      => Assert.AreEqual(filaAOkay, InOutFile.Create("a", true));

    [Test]
    public void Files_do_not_equal_when_one_is_not_okay()
      => Assert.AreNotEqual(filaAOkay, InOut.Create("a", false));

    [Test]
    public void Hash_code_equals_when_files_equal()
      => Assert.AreEqual(filaAOkay.GetHashCode(),
                         InOutFile.Create("a", true).GetHashCode());
  }
}