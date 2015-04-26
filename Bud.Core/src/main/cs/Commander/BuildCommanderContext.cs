using Bud.Logging;

namespace Bud.Commander {
  public class BuildCommanderContext {
    private readonly ILogger Logger;
    private IConfig CachedConfig;

    public BuildCommanderContext(Settings settings, ILogger logger) {
      Settings = settings;
      Logger = logger;
    }

    public Settings Settings { get; }

    public IConfig Config => CachedConfig ?? (CachedConfig = new Config(Settings.ConfigDefinitions, Logger));

    public IContext Context => Bud.Context.FromConfig(Config, Settings.TaskDefinitions);

    public BuildCommanderContext UpdateSettings(Settings newSettings) {
      return Settings == newSettings ? this : new BuildCommanderContext(newSettings, Logger);
    }
  }
}