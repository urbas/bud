using System.Diagnostics;
using Bud.Build;
using Bud.Test.Assertions;
using Bud.Test.Util;
using NUnit.Framework;

namespace Bud.SystemTests {
  public class TransitiveExternalDependencies {
    [Test]
    public void compile_MUST_produce_the_executable() {
      using (var buildCommander = TestProjects.LoadBuildCommander(this)) {
        buildCommander.EvaluateToJson("build");
        FileAssertions.FileExists(SystemTestUtils.OutputAssemblyPath(buildCommander, BuildKeys.Main, "A.dll"));
      }
    }
  }
}