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
        buildCommander.Evaluate(BuildKeys.Build);
        FileAssertions.AssertFileExists(BuiltAssemblyPath(buildCommander));
        buildCommander.Evaluate(BuildKeys.Clean);
        FileAssertions.AssertFileDoesNotExist(BuiltAssemblyPath(buildCommander));
      }
    }

    [Test]
    public void compile_MUST_produce_no_executable_WHEN_the_project_folder_is_empty() {
      using (var buildCommander = TestProjects.LoadBuildCommander()) {
        buildCommander.Evaluate(BuildKeys.Build);
        FileAssertions.AssertFileDoesNotExist(BuiltAssemblyPath(buildCommander));
      }
    }

    static string BuiltAssemblyPath(TemporaryDirBuildCommander buildCommander) {
      return Path.Combine(buildCommander.TemporaryDirectory.Path, ".bud", "output", ".net-4.5", "main", "debug", "bin", Path.GetFileName(buildCommander.TemporaryDirectory.Path) + ".exe");
    }
  }
}