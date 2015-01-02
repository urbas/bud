using System;
using System.Linq;
using Bud.Plugins.BuildLoading;
using Bud.Plugins.Projects;
using NUnit.Framework;
using Bud.Test.Assertions;
using System.IO;
using Bud.Plugins.Build;
using Bud.Test.Util;

namespace Bud.SystemTests {
  public class BuildConfigurationProject {
    [Test]
    public void Load_MUST_produce_the_build_assembly() {
      using (var buildCommander = TestProjects.LoadBuildCommander("BuildConfigurationProject")) {
        FileAssertions.AssertFileExists(Path.Combine(buildCommander.TemporaryDirectory.Path, BuildDirs.BudDirName, "Build.dll"));
      }
    }
  }
}

