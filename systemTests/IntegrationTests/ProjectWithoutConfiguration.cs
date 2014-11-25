using System;
using Bud.Test;
using Bud.Test.Util;
using NUnit.Framework;

namespace Bud.IntegrationTests {
  public class ProjectWithoutConfiguration {
    [Test]
    public void Executing_compile_MUST_produce_the_executable() {
      using (var testProjectCopy = TestProjects.TemporaryCopy("ProjectWithoutConfiguration")) {
        BuildConfiguration buildConfiguration = Bud.Load(testProjectCopy.Path);
        Bud.Execute(buildConfiguration, "compile");
        testProjectCopy.AssertFileExists(".bud/build/.net-4.0/main/debug/bin/program.exe");
      }
    }
  }
}