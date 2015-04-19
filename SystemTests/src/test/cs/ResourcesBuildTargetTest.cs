using Bud.CSharp;
using Bud.Test.Util;
using NUnit.Framework;

namespace Bud.SystemTests {
  public class ResourcesBuildTargetTest {
    [Test]
    public void run_MUST_print_out_the_contents_of_the_embedded_resource() {
      using (var buildCommander = TestProjects.LoadBuildCommander(this)) {
        var executionResult = buildCommander.EvaluateToJson("run");
        Assert.AreEqual(ExecutionResult.SuccessExitCode, executionResult);
        Assert.AreEqual("This is the content of an embedded resource file.", executionResult);
      }
    }
  }
}