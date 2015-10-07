using System;

namespace Bud.Configuration {
  public class ModifyConfig<T> : IConfigTransform {
    public string Key { get; }
    private Func<IConfigs, T, T> ValueFactory { get; }

    public ModifyConfig(Key<T> key, Func<IConfigs, T, T> valueFactory) {
      Key = key;
      ValueFactory = valueFactory;
    }

    public IConfigDefinition Modify(IConfigDefinition configDefinition) {
      return new ConfigDefinition<T>(configs => ValueFactory(configs, (T) configDefinition.Invoke(configs)));
    }

    public IConfigDefinition ToConfigDefinition() {
      throw new ConfigDefinitionException(Key, typeof(T), "Could not modify the configuration. No previous definition exists.");
    }
  }
}