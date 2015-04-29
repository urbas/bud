using System.Collections.Immutable;
using System.Threading.Tasks;
using Bud.Logging;
using Bud.SettingsConstruction;

namespace Bud.Evaluation {
  internal class ScopedConfig : IConfig {
    private readonly Config Config;

    public ScopedConfig(Config config, Key key) {
      Config = config;
      Logger = new ScopedLogger(config.Logger, key);
    }

    public ImmutableDictionary<ConfigKey, IConfigDefinition> ConfigDefinitions => Config.ConfigDefinitions;

    public ILogger Logger { get; }

    public bool IsConfigDefined(Key key) => Config.IsConfigDefined(key);

    public Task Evaluate(Key key) => Config.Evaluate(key);

    public T Evaluate<T>(ConfigKey<T> configKey) => Config.Evaluate(configKey);

    public bool TryEvaluate<T>(ConfigKey<T> configKey, out T evaluatedValue) => Config.TryEvaluate(configKey, out evaluatedValue);

    public object EvaluateConfig(Key key) => Config.EvaluateConfig(key);
  }
}