using Bud.Test.Util;
using Bud.Test.Assertions;
using System.IO;
using Bud.Plugins.Build;
using NUnit.Framework;

namespace Bud.SystemTests {
  public class ExternalDependenciesNoVersion {
    [Test]
    public void compile_MUST_produce_the_executable() {
      using (var buildCommander = TestProjects.LoadBuildCommander("ExternalDependenciesNoVersion")) {
        buildCommander.Evaluate("Build");
        FileAssertions.AssertFileExists(BuiltAssemblyPath(buildCommander, "Foo", ".exe"));
      }
    }

    static string BuiltAssemblyPath(TemporaryDirBuildCommander buildCommander, string projectName, string extension) {
      return Path.Combine(buildCommander.TemporaryDirectory.Path, BuildDirs.BudDirName, BuildDirs.OutputDirName, ".net-4.5", "main", "debug", "bin", projectName + extension);
    }
  }
}