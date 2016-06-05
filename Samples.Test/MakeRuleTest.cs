using Bud;
using NUnit.Framework;

namespace Samples {
  public class MakeRuleTest {
    [Test]
    public void Script_produces_an_output_file() {
      using (var dir = SampleDir.TmpCopy("MakeRule")) {
        BatchExec.RunQuietlyThrow(Paths.RunScript, "foo.out", dir.Path);
        FileAssert.AreEqual(dir.CreateFile("ThisisSparta!", "expected_output"),
                            dir.CreatePath("foo.out"));
      }
    }
  }
}