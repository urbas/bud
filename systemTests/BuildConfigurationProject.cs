using System;
using Bud.Plugins.BuildLoading;
using NUnit.Framework;

namespace Bud.SystemTests {
  public class BuildConfigurationProject {
    [Test]
    public void Loading_a_build_project_MUST_produce_the_compiled_configuration() {
      using (var testProjectCopy = TestProjects.TemporaryCopy("BuildConfigurationProject")) {
        var settings = BuildLoading.LoadBuildSettings(testProjectCopy.Path);
      }
    }
  }
}

