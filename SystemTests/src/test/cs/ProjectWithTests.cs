using Bud.Build;
using Bud.Test.Assertions;
using Bud.Test.Util;
using NUnit.Framework;

namespace Bud.SystemTests {
  public class ProjectWithTests {
    [Test]
    public void compile_MUST_produce_the_main_and_test_libraries() {
      using (var buildCommander = TestProjects.LoadBuildCommander(this)) {
        buildCommander.Evaluate("test");
        FileAssertions.FilesExist(SystemTestUtils.OutputAssemblyPath(buildCommander, "A", BuildKeys.Main, "A.exe"),
                                  SystemTestUtils.OutputAssemblyPath(buildCommander, "A", BuildKeys.Test, "A.Test.dll"));
      }
    }
  }
}