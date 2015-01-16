using Bud.Plugins.Build;
using Bud.Test.Assertions;
using Bud.Test.Util;
using NUnit.Framework;

namespace Bud.SystemTests {
  public class TestDependencies {
    [Test]
    public void compile_MUST_produce_the_executable() {
      using (var buildCommander = TestProjects.LoadBuildCommander(this)) {
        buildCommander.Evaluate("build");
        FileAssertions.AssertFileExists(SystemTestUtils.OutputAssemblyPath(buildCommander, BuildKeys.Test, "A.Test.dll"));
      }
    }
  }
}