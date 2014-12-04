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

        var compiledAssemblyFiles = CompiledAssemblyFiles(context);

        context.BuildAll().Wait();
        foreach (var assemblyFile in compiledAssemblyFiles) {
          FileAssertions.AssertFileExists(assemblyFile);
        }

        Assert.IsNotEmpty(compiledAssemblyFiles);

        context.CleanAll().Wait();
        foreach (var assemblyFile in compiledAssemblyFiles) {
          FileAssertions.AssertFileDoesNotExist(assemblyFile);
        }
      }
    }

    [Test]
    public void compile_MUST_produce_no_executable_WHEN_the_project_folder_is_empty() {
      using (var emptyProject = TestProjects.EmptyProject()) {
        var context = BuildLoading.Load(emptyProject.Path);
        context.Evaluate(BuildKeys.Build).Wait();
        var unexpectedCompiledFiles = CompiledAssemblyFiles(context);
        foreach (var assemblyFile in unexpectedCompiledFiles) {
          FileAssertions.AssertFileDoesNotExist(assemblyFile);
        }
      }
    }

    static System.Collections.Generic.IEnumerable<string> CompiledAssemblyFiles(EvaluationContext context) {
      return from project in context.GetAllProjects() select context.GetCSharpOutputAssemblyFile(project.Value);
    }
  }
}