using Bud.Test.Assertions;
using NUnit.Framework;
using System.Linq;
using Bud.Plugins.Build;
using Bud.Plugins.Dependencies;
using Bud.Plugins.Projects;
using Bud.Plugins.CSharp;
using Bud.Plugins.BuildLoading;
using System;
using Bud.Test.Util;
using Bud.Commander;
using System.IO;

namespace Bud.SystemTests {
  public class ProjectWithTests {
    [Test]
    public void compile_MUST_produce_the_main_and_test_libraries() {
      using (var buildCommander = TestProjects.LoadBuildCommander("ProjectWithTests")) {
        buildCommander.Evaluate("Test/Build");
        FileAssertions.AssertFilesExist(new [] {
          BuiltAssemblyPath(buildCommander, "main", "A.dll"),
          BuiltAssemblyPath(buildCommander, "test", "A.Test.dll")
        });
      }
    }

    static string BuiltAssemblyPath(TemporaryDirBuildCommander buildCommander, string scope, string assemblyFileName) {
      return Path.Combine(buildCommander.TemporaryDirectory.Path, assemblyFileName, BuildDirs.BudDirName, BuildDirs.OutputDirName, ".net-4.5", scope, "debug", "bin", assemblyFileName);
    }
  }
}