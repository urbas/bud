using System.Collections.Immutable;
using Bud.Logging;

namespace Bud {
  internal class ScopedConfig : IConfig {
    private readonly Config Config;

    public ScopedConfig(Config config, Key key) {
      Config = config;
      Logger = new ScopedLogger(config.Logger, key);
    }

    public ImmutableDictionary<ConfigKey, IConfigDefinition> ConfigDefinitions => Config.ConfigDefinitions;

    public ILogger Logger { get; }

    public bool IsConfigDefined(Key key) => Config.IsConfigDefined(key);

    public T Evaluate<T>(ConfigKey<T> configKey) => Config.Evaluate(configKey);

    public object EvaluateConfig(Key key) => Config.EvaluateConfig(key);
  }
}