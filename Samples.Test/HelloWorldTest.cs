using Bud;
using NUnit.Framework;

namespace Samples {
  [Category("IntegrationTest")]
  public class HelloWorldTest {
    [Test]
    public void Build_outputs_hello_world() {
      using (var dir = SampleDir.TmpCopy("HelloWorld")) {
        Assert.AreEqual("Hello, John Smith!",
                        BatchExec.GetOutputOrThrow(Paths.RunScript, "John Smith", dir.Path).Trim());
      }
    }
  }
}