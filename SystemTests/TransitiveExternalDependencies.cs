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
        buildCommander.Evaluate("fetch");
        Process.Start(buildCommander.TemporaryDirectory.Path);
        buildCommander.Evaluate("build");
        FileAssertions.AssertFilesExist(new[] {
          SystemTestUtils.OutputAssemblyPath(buildCommander, "A", BuildKeys.Main, "A.dll")
        });
      }
    }
  }
}