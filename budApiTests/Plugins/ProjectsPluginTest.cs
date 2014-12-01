using System;
using NUnit.Framework;
using Bud.Plugins;
using System.Collections.Immutable;

namespace Bud {
  public class ProjectsPluginTest {

    [Test]
    public void Create_MUST_add_the_project_to_the_list_of_projects() {
      var buildConfiguration = Project.New("foo", "./fooDir").ToEvaluationContext();
      var listOfProjects = buildConfiguration.Evaluate(ProjectPlugin.ListOfProjects);
      Assert.AreEqual(
        ImmutableHashSet.Create<SettingKey>().Add(Project.Key("foo")),
        listOfProjects
      );
    }

    [Test]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Create_MUST_not_insert_two_same_projects_to_the_list_of_projects() {
      Project.New("foo", "./fooDir").Add(Project.New("foo", "./fooDir")).ToEvaluationContext();
    }

    [Test]
    public void Create_MUST_insert_the_directory_of_the_project() {
      var buildConfiguration = Project.New("foo", "./fooDir").ToEvaluationContext();
      var baseDir = buildConfiguration.Evaluate(ProjectPlugin.BaseDir.In(Project.Key("foo")));
      Assert.AreEqual("./fooDir", baseDir);
    }

  }
}

