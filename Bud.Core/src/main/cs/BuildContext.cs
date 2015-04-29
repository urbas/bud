using System.Collections.Immutable;
using System.Threading.Tasks;
using Bud.Evaluation;
using Bud.Logging;
using Bud.SettingsConstruction;

namespace Bud {
  public class BuildContext : IContext {
    private IConfig CachedConfig;

    public BuildContext(Settings settings, ILogger logger) {
      Settings = settings;
      Logger = logger;
    }

    public Settings Settings { get; }

    public ImmutableDictionary<ConfigKey, IConfigDefinition> ConfigDefinitions => Config.ConfigDefinitions;

    public ILogger Logger { get; }

    public bool IsConfigDefined(Key key) => Config.IsConfigDefined(key);

    public Task Evaluate(Key key)
    {
      object value;
      return Config.TryEvaluate(key, out value) ? Task.FromResult(value) : Context.Evaluate((TaskKey)key);
    }

    public T Evaluate<T>(ConfigKey<T> configKey) => Config.Evaluate(configKey);

    public bool TryEvaluate<T>(ConfigKey<T> configKey, out T evaluatedValue) => Config.TryEvaluate(configKey, out evaluatedValue);

    public object EvaluateConfig(Key key) => Config.EvaluateConfig(key);

    public IConfig Config => CachedConfig ?? (CachedConfig = new Config(Settings.ConfigDefinitions, Logger));

    public IContext Context => Evaluation.Context.FromConfig(Config, Settings.TaskDefinitions);

    public BuildContext WithSettings(Settings newSettings) => Settings == newSettings ? this : new BuildContext(newSettings, Logger);

    public BuildContext ReloadConfig() {
      CachedConfig = null;
      return this;
    }

    public ImmutableDictionary<TaskKey, ITaskDefinition> TaskDefinitions => Context.TaskDefinitions;

    public bool IsTaskDefined(Key key) => Context.IsTaskDefined(key);

    public Task Evaluate(TaskKey key) => Context.Evaluate(key);

    public Task<T> Evaluate<T>(TaskKey<T> key) => Context.Evaluate(key);

    public object EvaluateKeySync(Key key) => Context.EvaluateKeySync(key);

    public Task<T> Evaluate<T>(TaskDefinition<T> taskDefinition, Key taskKey) => Context.Evaluate(taskDefinition, taskKey);

    public Task Evaluate(ITaskDefinition taskDefinition, Key taskKey) => Context.Evaluate(taskDefinition, taskKey);
  }
}