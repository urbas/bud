using System;
using NUnit.Framework;
using System.Collections.Immutable;
using Bud.Plugins.Projects;

namespace Bud {
  public class ProjectsPluginTest {

    [Test]
    public void Create_MUST_add_the_project_to_the_list_of_projects() {
      var context = Project.New("foo", "./fooDir").ToEvaluationContext();
      var listOfProjects = context.GetListOfProjects();
      Assert.AreEqual(
        ImmutableHashSet.Create<Scope>().Add(Project.Key("foo")),
        listOfProjects
      );
    }

    [Test]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Create_MUST_not_insert_two_same_projects_to_the_list_of_projects() {
      var projectFoo1 = Project.New("foo", "./fooDir");
      var projectFoo2 = Project.New("foo", "./fooDir");
      projectFoo1.Globally(s => s.Add(projectFoo2)).ToEvaluationContext();
    }

    [Test]
    public void Create_MUST_insert_the_directory_of_the_project() {
      var context = Project.New("foo", "./fooDir").ToEvaluationContext();
      var baseDir = context.GetBaseDir(Project.Key("foo"));
      Assert.AreEqual("./fooDir", baseDir);
    }

  }
}

