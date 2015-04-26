using Bud.Build;
using Bud.Test.Assertions;
using Bud.Test.Util;
using NUnit.Framework;

namespace Bud.SystemTests {
  public class TestDependencies {
    [Test]
    public void compile_MUST_produce_the_executable() {
      using (var buildCommander = TestProjects.LoadBuildCommander(this)) {
        buildCommander.EvaluateToJson("build");
        FileAssertions.FileExists(SystemTestUtils.OutputAssemblyPath(buildCommander, BuildKeys.Test, "A.Test.dll"));
      }
    }
  }
}