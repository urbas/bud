using System;
using Bud.Test;
using Bud.Test.Assertions;
using NUnit.Framework;

namespace Bud.SystemTests {
  public class ProjectWithoutConfiguration {
    const string EXPECTED_COMPILED_EXECUTABLE_PATH = ".net-4.5/main/debug/bin/program.exe";

    [Test]
    public void Executing_compile_MUST_produce_the_executable() {
      using (var testProjectCopy = TestProjects.TemporaryCopy("ProjectWithoutConfiguration")) {
        BuildConfiguration buildConfiguration = Bud.Load(testProjectCopy.Path);

        Bud.Evaluate(buildConfiguration, "compile");
        testProjectCopy.AssertOutputFileExists(EXPECTED_COMPILED_EXECUTABLE_PATH);

        Bud.Evaluate(buildConfiguration, "clean");
        testProjectCopy.AssertOutputFileDoesNotExist(EXPECTED_COMPILED_EXECUTABLE_PATH);
      }
    }
  }
}