using System;
using System.Linq;
using NUnit.Framework;
using System.Collections.Immutable;
using Bud.Plugins.Projects;

namespace Bud {
  public class ProjectsPluginTest {

    [Test]
    public void Create_MUST_add_the_project_to_the_list_of_projects() {
      var context = Project.New("foo", "./fooDir").ToEvaluationContext();
      var namesOfProjects = context.GetListOfProjects().Select(project => project.Id);
      Assert.AreEqual(new [] { "foo" }, namesOfProjects);
    }

    [Test]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Create_MUST_not_insert_two_same_projects_to_the_list_of_projects() {
      var projectFoo1 = Project.New("foo", "./fooDir");
      var projectFoo2 = Project.New("foo", "./fooDir");
      Settings.Start
        .Add(projectFoo1)
        .Add(projectFoo2)
        .ToEvaluationContext();
    }

    [Test]
    public void Create_MUST_insert_the_directory_of_the_project() {
      var context = Project.New("foo", "./fooDir").ToEvaluationContext();
      var projectsBaseDirs = context.GetListOfProjects().Select(project => context.GetBaseDir(project));
      Assert.AreEqual(new []{ "./fooDir" }, projectsBaseDirs);
    }

  }
}

