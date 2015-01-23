using System;
using System.Linq;
using NUnit.Framework;
using System.Collections.Immutable;
using Bud.Plugins.Projects;
using Bud.Plugins.Build;
using Bud;

namespace Bud {
  public class ProjectsPluginTest {

    [Test]
    public void AddProject_MUST_add_the_project_to_the_list_of_projects() {
      var context = CreateFakeProject();
      var namesOfProjects = context.GetAllProjects().Select(project => project.Key);
      Assert.AreEqual(new [] { "foo" }, namesOfProjects);
    }

    [Test]
    public void AddProject_MUST_insert_the_directory_of_the_project() {
      var context = CreateFakeProject();
      var projectsBaseDirs = context.GetAllProjects().Select(project => context.GetBaseDir(project.Value));
      Assert.AreEqual(new []{ "./fooDir" }, projectsBaseDirs);
    }

    private IContext CreateFakeProject() {
      return Context.FromSettings(GlobalBuild.New().Project("foo", "./fooDir"));
    }

  }
}

