using System.IO;
using Bud.BuildDefinition;
using Bud.Test.Assertions;
using Bud.Test.Util;
using NUnit.Framework;

namespace Bud.SystemTests {
  public class BuildLevels {
    [Test]
    public void Load_MUST_produce_the_build_definition_WITH_a_plugin_reference() {
      using (var buildCommander = TestProjects.LoadBuildCommander(this)) {
        FileAssertions.FileExists(Path.Combine(buildCommander.TemporaryDirectory.Path, BudPaths.BudDirName, BuildDefinitionPlugin.BuildDefinitionAssemblyFileName));
      }
    }
  }
}