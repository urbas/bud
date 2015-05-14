using Bud.Evaluation;
using Bud.Logging;

namespace Bud {
  internal class BuildContext : IBuildContext {
    private IConfig CachedConfig;

    public BuildContext(Settings settings, ILogger logger) {
      Settings = settings;
      Logger = logger;
    }

    public Settings Settings { get; }

    public ILogger Logger { get; }

    public IConfig Config => CachedConfig ?? (CachedConfig = new Config(Settings.ConfigDefinitions, Logger));

    public IContext Context => Evaluation.Context.FromConfig(Config, Settings.TaskDefinitions);

    public IBuildContext WithSettings(Settings newSettings) => Settings == newSettings ? this : new BuildContext(newSettings, Logger);

    public IBuildContext Reset() {
      CachedConfig = null;
      return this;
    }
  }
}