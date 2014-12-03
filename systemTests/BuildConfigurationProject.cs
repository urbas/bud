using System;
using System.Linq;
using Bud.Plugins.BuildLoading;
using Bud.Plugins.Projects;
using NUnit.Framework;

namespace Bud.SystemTests {
  public class BuildConfigurationProject {
    [Test]
    public void Load_MUST_return_the_projects_described_in_the_build_configuration_source_file() {
      using (var testProjectCopy = TestProjects.TemporaryCopy("BuildConfigurationProject")) {
        var settings = BuildLoading.Load(testProjectCopy.Path);
        var listOfProjectIds = settings.GetAllProjects().Select(project => project.Key);
        Assert.AreEqual(new [] {"A", "B"}, listOfProjectIds);
      }
    }
  }
}

