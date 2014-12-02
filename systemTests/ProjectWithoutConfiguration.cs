using System;
using System.Linq;
using Bud.Test;
using Bud.Test.Assertions;
using NUnit.Framework;
using Bud.Plugins.CSharp;
using Bud.Plugins;
using Bud.Plugins.Projects;

namespace Bud.SystemTests {
  public class ProjectWithoutConfiguration {
    [Test]
    public async void compile_MUST_produce_the_executable() {
      using (var testProjectCopy = TestProjects.TemporaryCopy("ProjectWithoutConfiguration")) {
        var context = BuildConfigurationLoader.Load(testProjectCopy.Path);

        var compiledAssemblyFiles = CompiledAssemblyFiles(context);

        await context.Evaluate(BuildPlugin.Build);
        foreach (var assemblyFile in compiledAssemblyFiles) {
          FileAssertions.AssertFileExists(assemblyFile);
        }

        Assert.IsNotEmpty(compiledAssemblyFiles);

        await context.Evaluate(BuildPlugin.Clean);
        foreach (var assemblyFile in compiledAssemblyFiles) {
          FileAssertions.AssertFileDoesNotExist(assemblyFile);
        }
      }
    }

    [Test]
    public async void compile_MUST_produce_no_executable_WHEN_the_project_folder_is_empty() {
      using (var emptyProject = TestProjects.EmptyProject()) {
        var context = BuildConfigurationLoader.Load(emptyProject.Path);
        await context.Evaluate(BuildPlugin.Build);
        var unexpectedCompiledFiles = CompiledAssemblyFiles(context);
        foreach (var assemblyFile in unexpectedCompiledFiles) {
          FileAssertions.AssertFileDoesNotExist(assemblyFile);
        }
      }
    }

    static System.Collections.Generic.IEnumerable<string> CompiledAssemblyFiles(EvaluationContext context) {
      return from project in context.GetListOfProjects() select context.GetCSharpOutputAssemblyFile(project);
    }
  }
}