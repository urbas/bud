using Bud.Plugins.CSharp;
using System.IO;
using Bud.Plugins.Build;

namespace Bud.Commander {
  public class DefaultBuildCommander : IBuildCommander {
    private readonly Settings settings;
    private readonly Config config;

    public DefaultBuildCommander(string dirOfProjectToBeBuilt) {
      settings = GlobalBuild.New(dirOfProjectToBeBuilt).CSharpProject(Path.GetFileName(dirOfProjectToBeBuilt), dirOfProjectToBeBuilt);
      config = new Config(settings.ConfigDefinitions);
    }

    public object Evaluate(string command) {
      var context = Context.FromConfig(config, settings.TaskDefinitions);
      return CommandEvaluator.EvaluateSynchronously(context, command);
    }

    public void Dispose() {
    }

  }
}
