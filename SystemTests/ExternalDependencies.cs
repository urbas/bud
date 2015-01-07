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
  public class ExternalDependencies {
    [Test]
    public void compile_MUST_produce_the_executable() {
      using (var buildCommander = TestProjects.LoadBuildCommander("ExternalDependencies")) {
        buildCommander.Evaluate("Fetch");
        buildCommander.Evaluate("Build");
        FileAssertions.AssertFileExists(BuiltAssemblyPath(buildCommander, "Foo", ".exe"));
      }
    }

    static string BuiltAssemblyPath(TemporaryDirBuildCommander buildCommander, string projectName, string extension) {
      return Path.Combine(buildCommander.TemporaryDirectory.Path, BuildDirs.BudDirName, BuildDirs.OutputDirName, ".net-4.5", "main", "debug", "bin", projectName + extension);
    }
  }
}