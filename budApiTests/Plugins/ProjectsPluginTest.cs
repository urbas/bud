using System;
using NUnit.Framework;
using Bud.Plugins;
using System.Collections.Immutable;

namespace Bud {
  public class ProjectsPluginTest {

    [Test]
    public void Create_MUST_add_the_project_to_the_list_of_projects() {
      var buildConfiguration = ProjectPlugin.Project("foo", ".").End();
      var listOfProjects = buildConfiguration.Evaluate(ProjectPlugin.ListOfProjects);
      Assert.AreEqual(
        ImmutableHashSet.Create<Project>().Add(new Project("foo", ".")),
        listOfProjects
      );
    }

    [Test]
    public void Create_MUST_not_insert_two_same_projects_to_the_list_of_projects() {
      var buildConfiguration = ProjectPlugin.Project("foo", ".").Add(ProjectPlugin.Project("foo", ".")).End();
      var listOfProjects = buildConfiguration.Evaluate(ProjectPlugin.ListOfProjects);
      Assert.AreEqual(
        ImmutableHashSet.Create<Project>().Add(new Project("foo", ".")),
        listOfProjects
      );
    }

  }
}

