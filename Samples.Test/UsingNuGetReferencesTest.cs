using Bud;
using NUnit.Framework;

namespace Samples {
  public class UsingNuGetReferencesTest {
    [Test]
    public void Build_outputs_some_json() {
      using (var dir = SampleDir.TmpCopy("UsingNuGetReferences")) {
        Assert.AreEqual(@"[42,""answer""]",
                        BatchExec.GetOutputOrThrow(Paths.RunScript, "", dir.Path).Trim());
      }
    }
  }
}