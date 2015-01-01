﻿using System;
using System.Linq;
using NUnit.Framework;
using System.Collections.Immutable;
using Bud.Plugins.Projects;
using Bud.Plugins.Build;
using Bud;

namespace Bud {
  public class ProjectsPluginTest {

    public Settings CreateFakeProject() {
      return GlobalBuild.New(".").AddProject("foo", "./fooDir");
    }

    [Test]
    public void Create_MUST_add_the_project_to_the_list_of_projects() {
      var context = EvaluationContext.FromSettings(CreateFakeProject());
      var namesOfProjects = context.GetAllProjects().Select(project => project.Key);
      Assert.AreEqual(new [] { "foo" }, namesOfProjects);
    }

    [Test]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Create_MUST_not_insert_two_same_projects_to_the_list_of_projects() {
      EvaluationContext.FromSettings(CreateFakeProject().AddProject("foo", "./fooDir"));
    }

    [Test]
    public void Create_MUST_insert_the_directory_of_the_project() {
      var context = EvaluationContext.FromSettings(CreateFakeProject());
      var projectsBaseDirs = context.GetAllProjects().Select(project => context.GetBaseDir(project.Value));
      Assert.AreEqual(new []{ "./fooDir" }, projectsBaseDirs);
    }

  }
}

