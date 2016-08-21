using Bud;
using NUnit.Framework;

namespace Samples {
  [Category("IntegrationTest")]
  public class MakeRuleTest {
    [Test]
    public void Script_produces_an_output_file() {
      using (var dir = SampleDir.TmpCopy("MakeRule")) {
        Exec.Run(Paths.RunScript, "", dir.Path);
        FileAssert.AreEqual(dir.CreateFile("ThisisSparta!", "expected_output"),
                            dir.CreatePath("foo.out"));
      }
    }
  }
}