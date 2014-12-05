using Bud.Test.Assertions;
using NUnit.Framework;
using System.Linq;
using Bud.Plugins.Build;
using Bud.Plugins.Projects;
using Bud.Plugins.CSharp;
using Bud.Plugins.BuildLoading;
using System;

namespace Bud.SystemTests {
  public class ProjectWithoutConfiguration {
    [Test]
    public void compile_MUST_produce_the_executable() {
      using (var testProjectCopy = TestProjects.TemporaryCopy("ProjectWithoutConfiguration")) {
        var context = BuildLoading.Load(testProjectCopy.Path);

        context.BuildAll().Wait();
        FileAssertions.AssertFilesExist(CompiledAssemblyFiles(context));
        Assert.IsNotEmpty(CompiledAssemblyFiles(context));

        context.CleanAll().Wait();
        FileAssertions.AssertFilesDoNotExist(CompiledAssemblyFiles(context));
      }
    }

    [Test]
    public void compile_MUST_produce_no_executable_WHEN_the_project_folder_is_empty() {
      using (var emptyProject = TestProjects.EmptyProject()) {
        var context = BuildLoading.Load(emptyProject.Path);
        context.Evaluate(BuildKeys.Build).Wait();
        FileAssertions.AssertFilesDoNotExist(CompiledAssemblyFiles(context));
      }
    }

    static System.Collections.Generic.IEnumerable<string> CompiledAssemblyFiles(EvaluationContext context) {
      return context
        .GetAllProjects()
        .Select(project => context.GetCSharpOutputAssemblyFile(project.Value));
    }
  }
}