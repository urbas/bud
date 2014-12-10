using Bud.Plugins.CSharp;
using System.IO;

namespace Bud.Commander {
  public class DefaultBuildCommander : IBuildCommander {
    Settings settings;

    public DefaultBuildCommander(string dirOfProjectToBeBuilt) {
      settings = CSharp.Project(Path.GetFileName(dirOfProjectToBeBuilt), dirOfProjectToBeBuilt);
    }

    public string Evaluate(string command) {
      return CommandEvaluator.Evaluate(settings, command);
    }

    public void Dispose() {
    }

  }
}
