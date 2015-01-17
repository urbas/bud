using System.Diagnostics;
using Bud.Plugins.Build;
using Bud.Test.Assertions;
using Bud.Test.Util;
using NUnit.Framework;

namespace Bud.SystemTests {
  public class TransitiveExternalDependencies {
    [Test]
    public void compile_MUST_produce_the_executable() {
      using (var buildCommander = TestProjects.LoadBuildCommander(this)) {
        buildCommander.Evaluate("build");
        FileAssertions.AssertFilesExist(new[] {
          SystemTestUtils.OutputAssemblyPath(buildCommander, BuildKeys.Main, "A.dll")
        });
      }
    }
  }
}