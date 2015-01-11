using Bud.Test.Util;
using Bud.Test.Assertions;
using NUnit.Framework;

namespace Bud.SystemTests {
  public class ExternalDependenciesNoVersion {
    [Test]
    public void compile_MUST_produce_the_executable() {
      using (var buildCommander = TestProjects.LoadBuildCommander("ExternalDependenciesNoVersion")) {
        buildCommander.Evaluate("build");
        FileAssertions.AssertFileExists(SystemTestUtils.OutputAssemblyPath(buildCommander, "Foo.exe"));
      }
    }
  }
}