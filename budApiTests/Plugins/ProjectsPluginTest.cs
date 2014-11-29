using System;
using NUnit.Framework;
using Bud.Plugins;
using System.Collections.Immutable;

namespace Bud {
  public class ProjectsPluginTest {

    [Test]
    public void Create_MUST_add_the_project_to_the_list_of_projects() {
      var buildConfiguration = ProjectPlugin.Project("foo", "./fooDir").End();
      var listOfProjects = buildConfiguration.Evaluate(ProjectPlugin.ListOfProjects);
      Assert.AreEqual(
        ImmutableHashSet.Create<Project>().Add(new Project("foo")),
        listOfProjects
      );
    }

    [Test]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Create_MUST_not_insert_two_same_projects_to_the_list_of_projects() {
      ProjectPlugin.Project("foo", "./fooDir").Add(ProjectPlugin.Project("foo", "./fooDir")).End();
    }

    [Test]
    public void Create_MUST_insert_the_directory_of_the_project() {
      var buildConfiguration = ProjectPlugin.Project("foo", "./fooDir").End();
      var baseDir = buildConfiguration.Evaluate(ProjectPlugin.BaseDir.In(new Project("foo")));
      Assert.AreEqual("./fooDir", baseDir);
    }

  }
}

