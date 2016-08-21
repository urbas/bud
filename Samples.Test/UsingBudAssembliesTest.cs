using Bud;
using NUnit.Framework;

namespace Samples {
  [Category("IntegrationTest")]
  public class UsingBudAssembliesTest {
    [Test]
    public void Build_outputs_a_text_file() {
      using (var dir = SampleDir.TmpCopy("UsingBudAssemblies")) {
        Assert.AreEqual("This is an option: Some<System.Int32>(42)!",
                        Exec.CheckOutput(Paths.RunScript, cwd: dir.Path).Trim());
      }
    }
  }
}