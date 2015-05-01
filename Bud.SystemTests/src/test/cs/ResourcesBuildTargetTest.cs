using Bud.Commander;
using Bud.CSharp;
using Bud.Test.Util;
using NUnit.Framework;

namespace Bud.SystemTests {
  public class ResourcesBuildTargetTest {
    [Test]
    public void run_MUST_print_out_the_contents_of_the_embedded_resource() {
      using (var buildCommander = TestProjects.Load(this)) {
        var executionResult = buildCommander.Evaluate<ExecutionResult>("/project/Foo/main/cs/run");
        Assert.AreEqual(ExecutionResult.SuccessExitCode, executionResult.ExitCode);
        Assert.AreEqual("This is the content of an embedded resource file.Another content of another test resource file.", executionResult.Output);
      }
    }
  }
}