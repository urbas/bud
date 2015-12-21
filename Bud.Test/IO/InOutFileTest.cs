using NUnit.Framework;

namespace Bud.IO {
  public class InOutFileTest {
    private readonly InOutFile filaAOkay = InOutFile.ToInOutFile("a");

    [Test]
    public void Path_is_stored() => Assert.AreEqual("a", filaAOkay.Path);

    [Test]
    public void Files_equal()
      => Assert.AreEqual(filaAOkay, InOutFile.ToInOutFile("a"));

    [Test]
    public void Different_files_do_not_equal()
      => Assert.AreNotEqual(filaAOkay, InOutFile.ToInOutFile("b"));

    [Test]
    public void Hash_code_equals_when_files_equal()
      => Assert.AreEqual(filaAOkay.GetHashCode(),
                         InOutFile.ToInOutFile("a").GetHashCode());
  }
}