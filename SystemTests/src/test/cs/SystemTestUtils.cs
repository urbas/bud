using System.IO;
using Bud.Build;
using Bud.CSharp;
using Bud.Test.Util;

namespace Bud {
  static internal class SystemTestUtils {
    public static string OutputAssemblyPath(TemporaryDirBuildCommander buildCommander, string fileName) {
      return OutputAssemblyPath(buildCommander, BuildKeys.Main, fileName);
    }

    public static string OutputAssemblyPath(TemporaryDirBuildCommander buildCommander, Key buildScope, string fileName) {
      var projectBaseDir = buildCommander.TemporaryDirectory.Path;
      return OutputAssemblyPath(buildScope, projectBaseDir, fileName);
    }

    public static string OutputAssemblyPath(TemporaryDirBuildCommander buildCommander, string projectName, Key buildScope, string fileName) {
      var projectBaseDir = Path.Combine(buildCommander.TemporaryDirectory.Path, projectName);
      return OutputAssemblyPath(buildScope, projectBaseDir, fileName);
    }

    private static string OutputAssemblyPath(Key buildScope, string projectBaseDir, string fileName) {
      var projectOutputSubPath = Path.Combine(BuildDirs.BudDirName, BuildDirs.OutputDirName, buildScope.Id, CSharpKeys.CSharp.Id, "debug", "bin", fileName);
      return Path.Combine(projectBaseDir, projectOutputSubPath);
    }
  }
}