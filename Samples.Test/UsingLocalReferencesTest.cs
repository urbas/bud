using Bud;
using NUnit.Framework;

namespace Samples {
  [Category("IntegrationTest")]
  public class UsingLocalReferencesTest {
    [Test]
    public void Build_outputs_some_json() {
      using (var dir = SampleDir.TmpCopy("UsingLocalReferences")) {
        Assert.AreEqual("foo bar",
                        BatchExec.GetOutputOrThrow(Paths.RunScript, "", dir.Path).Trim());
      }
    }
  }
}