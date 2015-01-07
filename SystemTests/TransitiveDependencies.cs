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
  public class TransitiveDependencies {
    [Test]
    public void compile_MUST_produce_the_executable() {
      using (var buildCommander = TestProjects.LoadBuildCommander("TransitiveDependencies")) {
        buildCommander.Evaluate("Build");
        FileAssertions.AssertFilesExist(new [] {
          BuiltAssemblyPath(buildCommander, "Common", ".dll"),
          BuiltAssemblyPath(buildCommander, "A", ".dll"),
          BuiltAssemblyPath(buildCommander, "B", ".exe")
        });
      }
    }

    static string BuiltAssemblyPath(TemporaryDirBuildCommander buildCommander, string projectName, string extension) {
      return Path.Combine(buildCommander.TemporaryDirectory.Path, projectName, BuildDirs.BudDirName, BuildDirs.OutputDirName, ".net-4.5", "main", "debug", "bin", projectName + extension);
    }
  }
}