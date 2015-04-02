using Bud.Build;
using Bud.Test.Assertions;
using Bud.Test.Util;
using NUnit.Framework;

namespace Bud.SystemTests {
  public class SimpleProjectTest {
    [Test]
    public void compile_MUST_produce_the_executable() {
      using (var buildCommander = TestProjects.LoadBuildCommander(this)) {
        buildCommander.Evaluate("build");
        FileAssertions.FilesExist(SystemTestUtils.OutputAssemblyPath(buildCommander, BuildKeys.Main, "Foo.exe"));
      }
    }
  }
}