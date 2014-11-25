using NUnit.Framework;
using System;
using Bud.Test.Util;
using Bud.Test;
using System.Threading.Tasks;

namespace Bud.IntegrationTests {
  public class NoConfiguration {
    [Test]
    public void TestCase() {
      using (var testProjectCopy = TestProjects.TemporaryCopy("NoConfiguration")) {
        BuildConfiguration buildConfiguration = Bud.Load(testProjectCopy.Path);
        Bud.ExecuteTask(buildConfiguration, "compile");
        testProjectCopy.AssertFileExists(".bud/build/.net-4.0/main/debug/bin/program.exe");
      }
    }
  }
}