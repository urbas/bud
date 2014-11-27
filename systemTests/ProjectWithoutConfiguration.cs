using System;
using Bud.Test;
using Bud.Test.Assertions;
using NUnit.Framework;
using Bud.Plugin.CSharp;

namespace Bud.SystemTests {
  public class ProjectWithoutConfiguration {
    [Test]
    public void compile_MUST_produce_the_executable() {
      using (var testProjectCopy = TestProjects.TemporaryCopy("ProjectWithoutConfiguration")) {
        var buildConfiguration = Bud.Load(testProjectCopy.Path);

        var expectedCompiledFile = CSharpPlugin.GetDefaultOutputFile(testProjectCopy.Path);

        Bud.Evaluate(buildConfiguration, "compile");
        FileAssertions.AssertFileExists(expectedCompiledFile);

        Bud.Evaluate(buildConfiguration, "clean");
        FileAssertions.AssertFileDoesNotExist(expectedCompiledFile);
      }
    }

    [Test]
    public void compile_MUST_produce_no_executable_WHEN_the_project_folder_is_empty() {
      using (var emptyProject = TestProjects.EmptyProject()) {
        var buildConfiguration = Bud.Load(emptyProject.Path);
        Bud.Evaluate(buildConfiguration, "compile");

        var compiledFile = CSharpPlugin.GetDefaultOutputFile(emptyProject.Path);
        FileAssertions.AssertFileDoesNotExist(compiledFile);
      }
    }
  }
}