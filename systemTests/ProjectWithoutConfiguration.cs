using System;
using Bud.Test;
using Bud.Test.Assertions;
using NUnit.Framework;

namespace Bud.SystemTests {
  public class ProjectWithoutConfiguration {
    [Test]
    public void Executing_compile_MUST_produce_the_executable() {
      using (var testProjectCopy = TestProjects.TemporaryCopy("ProjectWithoutConfiguration")) {
        BuildConfiguration buildConfiguration = Bud.Load(testProjectCopy.Path);
        Bud.Evaluate(buildConfiguration, "compile");
        testProjectCopy.AssertOutputFileExists(".net-4.5/main/debug/bin/program.exe");
        Bud.Evaluate(buildConfiguration, "clean");
        testProjectCopy.AssertOutputFileDoesNotExist(".net-4.5/main/debug/bin/program.exe");
      }
    }
  }
}