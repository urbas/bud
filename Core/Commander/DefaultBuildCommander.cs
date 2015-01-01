using Bud.Plugins.CSharp;
using System.IO;
using Bud.Plugins.Build;

namespace Bud.Commander {
  public class DefaultBuildCommander : IBuildCommander {
    Settings settings;

    public DefaultBuildCommander(string dirOfProjectToBeBuilt) {
      settings = GlobalBuild.New(dirOfProjectToBeBuilt).CSharpProject(Path.GetFileName(dirOfProjectToBeBuilt), dirOfProjectToBeBuilt);
    }

    public object Evaluate(string command) {
      return CommandEvaluator.Evaluate(settings, command);
    }

    public void Dispose() {
    }

  }
}
