using Bud;
using NUnit.Framework;

namespace Samples {
  [Category("IntegrationTest")]
  public class UsingFrameworkAssembliesTest {
    [Test]
    public void Build_outputs_shouts() {
      using (var dir = SampleDir.TmpCopy("UsingFrameworkAssemblies")) {
        Assert.AreEqual("Hello, JOHN SMITH!",
                        Exec.CheckOutput(Paths.RunScript, "John Smith", dir.Path).Trim());
      }
    }
  }
}