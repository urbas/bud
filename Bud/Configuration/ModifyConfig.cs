using System;

namespace Bud.Configuration {
  public class ModifyConfig<T> : ConfigTransform<T> {
    public ModifyConfig(Key<T> key, Func<IConfigs, T, T> valueFactory) : base(key) {
      ValueFactory = valueFactory;
    }

    private Func<IConfigs, T, T> ValueFactory { get; }
    public override ConfigDefinition<T> Modify(ConfigDefinition<T> configDefinition) => new ConfigDefinition<T>(configs => ValueFactory(configs, configDefinition.Invoke(configs)));

    public override ConfigDefinition<T> ToConfigDefinition() {
      throw new ConfigDefinitionException(Key, typeof(T), "Could not modify the configuration. No previous definition exists.");
    }
  }
}