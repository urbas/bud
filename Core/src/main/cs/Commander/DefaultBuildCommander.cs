using System.IO;
using Bud.Build;
using Bud.CSharp;
using Bud.Projects;

namespace Bud.Commander {
  public class DefaultBuildCommander : IBuildCommander {
    private readonly Settings settings;
    private readonly Config config;

    public DefaultBuildCommander(string dirOfProjectToBeBuilt) {
      settings = GlobalBuild.New(dirOfProjectToBeBuilt)
                            .Project(Path.GetFileName(dirOfProjectToBeBuilt), dirOfProjectToBeBuilt, Cs.Exe(), Cs.Test());
      config = new Config(settings.ConfigDefinitions);
    }

    public object Evaluate(string command) {
      var context = Context.FromConfig(config, settings.TaskDefinitions);
      return CommandEvaluator.EvaluateSynchronously(context, command);
    }

    public void Dispose() {}
  }
}