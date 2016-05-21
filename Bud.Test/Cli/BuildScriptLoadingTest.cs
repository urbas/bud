using Bud.IO;
using Bud.TempDir;
using Bud.V1;
using NUnit.Framework;
using static Bud.Cli.BuildScriptLoading;
using static Bud.V1.Basic;
using static NUnit.Framework.Assert;

namespace Bud.Cli {
  public class BuildScriptLoadingTest {
    [Test]
    public void LoadBuildDefinition_initializes_the_BaseDir() {
      using (var baseDir = new TemporaryDirectory()) {
        var buildDefinition = LoadBuildDefinition(BuildConf, baseDir.Path);
        AreEqual(baseDir.Path,
                 buildDefinition.Get(BaseDir));
      }
    }

    [Test]
    public void LoadBuildDefinition_initializes_the_BaseDir_in_projects() {
      using (var baseDir = new TemporaryDirectory()) {
        var buildDefinition = LoadBuildDefinition(BuildConf, baseDir.Path);
        AreEqual(baseDir.Path,
                 buildDefinition.Get("A"/BaseDir));
      }
    }

    private static Conf BuildConf { get; } = Projects(Project("A"));
  }
}