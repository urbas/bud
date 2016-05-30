using Bud;
using NUnit.Framework;

namespace Samples {
  public class UsingFrameworkAssembliesTest {
    [Test]
    public void Build_outputs_shouts() {
      using (var dir = SampleDir.TmpCopy("UsingFrameworkAssemblies")) {
        Assert.AreEqual("Hello, JOHN SMITH!",
                        BatchExec.GetOutputOrThrow(Paths.RunScript, "John Smith", dir.Path).Trim());
      }
    }
  }
}