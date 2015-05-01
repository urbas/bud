using System.IO;
using Bud.Test.Assertions;
using Bud.Test.Util;
using NUnit.Framework;

namespace Bud.SystemTests {
  public class ProjectWithoutConfiguration {
    [Test]
    public void compile_MUST_produce_the_executable() {
      using (var buildCommander = TestProjects.Load(this)) {
        buildCommander.EvaluateToJson("build");
        FileAssertions.FileExists(OutputAssemblyPath(buildCommander));
        buildCommander.EvaluateToJson("clean");
        FileAssertions.FileDoesNotExist(OutputAssemblyPath(buildCommander));
      }
    }

    [Test]
    public void compile_MUST_produce_no_executable_WHEN_the_project_folder_is_empty() {
      using (var buildCommander = TestProjects.Load()) {
        buildCommander.EvaluateToJson("build");
        FileAssertions.FileDoesNotExist(OutputAssemblyPath(buildCommander));
      }
    }

    private static string OutputAssemblyPath(TemporaryDirBuildCommander buildCommander) {
      return SystemTestUtils.OutputAssemblyPath(buildCommander, Path.GetFileName(buildCommander.TemporaryDirectory.Path) + ".exe");
    }
  }
}