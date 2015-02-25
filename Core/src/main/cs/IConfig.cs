using System.Collections.Immutable;
using Bud.Logging;

namespace Bud {
  public interface IConfig {
    ImmutableDictionary<Key, IConfigDefinition> ConfigDefinitions { get; }
    ILogger Logger { get; }
    bool IsConfigDefined(Key key);
    T Evaluate<T>(ConfigKey<T> configKey);
    object EvaluateConfig(Key key);
  }
}