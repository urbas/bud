using System.IO;
using Bud.Build;
using Bud.CSharp;
using Bud.Logging;
using Bud.Projects;

namespace Bud.Commander {
  public class DefaultBuildCommander : IBuildCommander {
    private readonly Settings Settings;
    private readonly Config Config;

    public DefaultBuildCommander(string dirOfProjectToBeBuilt)
      : this(CreateDefaultSettings(dirOfProjectToBeBuilt)) {}

    private static Settings CreateDefaultSettings(string dirOfProjectToBeBuilt) {
      return GlobalBuild.New(dirOfProjectToBeBuilt)
                        .Project(Path.GetFileName(dirOfProjectToBeBuilt), dirOfProjectToBeBuilt, Cs.Exe(), Cs.Test());
    }

    public DefaultBuildCommander(Settings settings) {
      Settings = settings;
      Config = new Config(settings.ConfigDefinitions, Logger.CreateFromStandardOutputs());
    }

    public object Evaluate(string command) {
      var context = Context.FromConfig(Config, Settings.TaskDefinitions);
      return CommandEvaluator.EvaluateSynchronously(context, command);
    }

    public void Dispose() {}
  }
}