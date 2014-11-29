using System;
using Bud.Test;
using Bud.Test.Assertions;
using NUnit.Framework;
using Bud.Plugins.CSharp;
using Bud.Plugins;

namespace Bud.SystemTests {
  public class ProjectWithoutConfiguration {
    [Test]
    public void compile_MUST_produce_the_executable() {
      using (var testProjectCopy = TestProjects.TemporaryCopy("ProjectWithoutConfiguration")) {
        var buildConfiguration = BuildConfigurationLoader.Load(testProjectCopy.Path);

        var expectedCompiledFile = buildConfiguration.GetCSharpOutputAssemblyFile(new Project(testProjectCopy.Path));

        buildConfiguration.Evaluate(CSharpPlugin.CSharpBuild);
        FileAssertions.AssertFileExists(expectedCompiledFile);

        buildConfiguration.Evaluate(BuildPlugin.Clean);
        FileAssertions.AssertFileDoesNotExist(expectedCompiledFile);
      }
    }

    [Test]
    public void compile_MUST_produce_no_executable_WHEN_the_project_folder_is_empty() {
      using (var emptyProject = TestProjects.EmptyProject()) {
        var buildConfiguration = BuildConfigurationLoader.Load(emptyProject.Path);
        buildConfiguration.Evaluate(CSharpPlugin.CSharpBuild);
        var unexpectedCompiledFile = buildConfiguration.GetCSharpOutputAssemblyFile(new Project(emptyProject.Path));
        FileAssertions.AssertFileDoesNotExist(unexpectedCompiledFile);
      }
    }
  }
}