using System;
using System.Linq;
using Bud.Plugins.BuildLoading;
using Bud.Plugins.Projects;
using NUnit.Framework;
using Bud.Test.Assertions;
using System.IO;

namespace Bud.SystemTests {
  public class BuildConfigurationProject {
    [Test]
    public void Load_MUST_produce_the_build_assembly() {
      using (var testProjectCopy = TestProjects.TemporaryCopy("BuildConfigurationProject")) {
        var settings = BuildLoading.Load(testProjectCopy.Path);
        FileAssertions.AssertFileExists(Path.Combine(testProjectCopy.Path, ".bud", ".bud", "output", ".net-4.5", "main", "debug", "bin", "BuildDefinition.dll"));
      }
    }
  }
}

