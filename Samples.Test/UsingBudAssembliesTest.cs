using Bud;
using NUnit.Framework;

namespace Samples {
  public class UsingBudAssembliesTest {
    [Test]
    public void Build_outputs_a_text_file() {
      using (var dir = SampleDir.TmpCopy("UsingBudAssemblies")) {
        Assert.AreEqual("This is an option: Some<System.Int32>(42)!",
                        BatchExec.GetOutputOrThrow(Paths.RunScript, workingDir: dir.Path).Trim());
      }
    }
  }
}