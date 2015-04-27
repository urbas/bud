using Bud.Logging;

namespace Bud {
  public class BuildContext {
    private IConfig CachedConfig;

    public BuildContext(Settings settings, ILogger logger) {
      Settings = settings;
      Logger = logger;
    }

    public Settings Settings { get; }

    public ILogger Logger { get; }

    public IConfig Config => CachedConfig ?? (CachedConfig = new Config(Settings.ConfigDefinitions, Logger));

    public IContext Context => Bud.Context.FromConfig(Config, Settings.TaskDefinitions);

    public BuildContext WithSettings(Settings newSettings) => Settings == newSettings ? this : new BuildContext(newSettings, Logger);

    public BuildContext ReloadConfig() {
      CachedConfig = null;
      return this;
    } 
  }
}