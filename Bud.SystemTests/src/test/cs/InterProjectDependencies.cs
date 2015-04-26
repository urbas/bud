using Bud.Build;
using Bud.Test.Assertions;
using Bud.Test.Util;
using NUnit.Framework;

namespace Bud.SystemTests {
  public class InterProjectDependencies {
    [Test]
    public void compile_MUST_produce_the_executable() {
      using (var buildCommander = TestProjects.LoadBuildCommander(this)) {
        buildCommander.EvaluateToJson("build");
        FileAssertions.FilesExist(
          SystemTestUtils.OutputAssemblyPath(buildCommander, "A", BuildKeys.Main, "A.dll"),
          SystemTestUtils.OutputAssemblyPath(buildCommander, "B", BuildKeys.Main, "B.exe")
        );
      }
    }
  }
}