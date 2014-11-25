using NUnit.Framework;
using System;
using Bud.Test.Util;

namespace Bud.IntegrationTests {
  public class NoConfiguration {
    [Test]
    public void TestCase() {
      using (var testProjectCopy = TestProjects.TemporaryCopy("NoConfiguration")) {
        BuildConfiguration bud = Bud.LoadBuildConfiguration(testProjectCopy.Path);
      }
    }
  }
}