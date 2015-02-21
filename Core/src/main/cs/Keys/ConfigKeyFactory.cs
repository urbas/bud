using System.Collections.Immutable;

namespace Bud.Keys {
  internal sealed class ConfigKeyFactory<T> : IKeyFactory<ConfigKey<T>> {
    public static readonly IKeyFactory<ConfigKey<T>> Instance = new ConfigKeyFactory<T>();

    private ConfigKeyFactory() {}

    public ConfigKey<T> Define(ImmutableList<string> path, string description) {
      return new ConfigKey<T>(path, description);
    }
  }
}