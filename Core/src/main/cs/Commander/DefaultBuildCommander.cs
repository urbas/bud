using Bud.Build;
using Bud.Logging;

namespace Bud.Commander {
  public class DefaultBuildCommander : IBuildCommander {
    private readonly Settings Settings;
    private readonly Config Config;

    public DefaultBuildCommander(string dirOfProjectToBeBuilt)
      : this(CreateDefaultSettings(dirOfProjectToBeBuilt)) {}

    public DefaultBuildCommander(Settings settings) {
      Settings = settings;
      Config = new Config(settings.ConfigDefinitions, Logger.CreateFromStandardOutputs());
    }

    public string EvaluateToJson(string command) {
      var context = Context.FromConfig(Config, Settings.TaskDefinitions);
      return CommandEvaluator.EvaluateToJsonSynchronously(context, command);
    }

    public void Dispose() {}

    private static Settings CreateDefaultSettings(string dirOfProjectToBeBuilt) {
      var globalSettings = GlobalBuild.New(dirOfProjectToBeBuilt);
      return new DefaultBuild().Setup(globalSettings, dirOfProjectToBeBuilt);
    }
  }
}