using Bud.Build;
using Bud.Logging;

namespace Bud.Commander {
  public class DefaultBuildCommander : IBuildCommander {
    private BuildContext BuildContext;

    public DefaultBuildCommander(string dirOfProjectToBeBuilt, bool isQuiet)
      : this(CreateDefaultSettings(dirOfProjectToBeBuilt), isQuiet) {}

    public DefaultBuildCommander(Settings settings, bool isQuiet) {
      var logger = isQuiet ? Logger.NullLogger : Logger.CreateFromStandardOutputs();
      BuildContext = new BuildContext(settings, logger);
    }

    public string EvaluateToJson(string command) => CommandEvaluator.EvaluateToJsonSync(command, ref BuildContext);

    public void Dispose() {}

    private static Settings CreateDefaultSettings(string dirOfProjectToBeBuilt) {
      var globalSettings = GlobalBuild.New(dirOfProjectToBeBuilt);
      return new DefaultBuild().Setup(globalSettings, dirOfProjectToBeBuilt);
    }
  }
}