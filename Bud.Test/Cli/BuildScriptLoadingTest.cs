using Bud.IO;
using Bud.V1;
using NUnit.Framework;
using static Bud.Cli.BuildScriptLoading;
using static Bud.V1.Api;
using static NUnit.Framework.Assert;

namespace Bud.Cli {
  public class BuildScriptLoadingTest {
    [Test]
    public void LoadBuildDefinition_initializes_the_BaseDir() {
      using (var tmpDir = new TemporaryDirectory()) {
        var buildDefinition = LoadBuildDefinition(BuildConf, tmpDir.Path);
        AreEqual(tmpDir.Path,
                 buildDefinition.Get(BaseDir));
      }
    }

    [Test]
    public void LoadBuildDefinition_initializes_the_BaseDir_in_projects() {
      using (var tmpDir = new TemporaryDirectory()) {
        var buildDefinition = LoadBuildDefinition(BuildConf, tmpDir.Path);
        AreEqual(tmpDir.Path,
                 buildDefinition.Get("A" / BaseDir));
      }
    }

    private static Conf BuildConf { get; } = Projects(BareProject("A"));
  }
}