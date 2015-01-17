using Bud.Commander;
using Bud.Test.Util;
using Bud.Plugins.Build;
using Bud.Test.Assertions;
using System.IO;
using NUnit.Framework;

namespace Bud.SystemTests {
  public class ProjectWithoutConfiguration {
    [Test]
    public void compile_MUST_produce_the_executable() {
      using (var buildCommander = TestProjects.LoadBuildCommander("ProjectWithoutConfiguration")) {
        buildCommander.Evaluate("build");
        FileAssertions.AssertFileExists(OutputAssemblyPath(buildCommander));
        buildCommander.Evaluate("clean");
        FileAssertions.AssertFileDoesNotExist(OutputAssemblyPath(buildCommander));
      }
    }

    [Test]
    public void compile_MUST_produce_no_executable_WHEN_the_project_folder_is_empty() {
      using (var buildCommander = TestProjects.LoadBuildCommander()) {
        buildCommander.Evaluate("build");
        FileAssertions.AssertFileDoesNotExist(OutputAssemblyPath(buildCommander));
      }
    }

    private static string OutputAssemblyPath(TemporaryDirBuildCommander buildCommander) {
      return SystemTestUtils.OutputAssemblyPath(buildCommander, Path.GetFileName(buildCommander.TemporaryDirectory.Path) + ".exe");
    }
  }
}