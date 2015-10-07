using System;

namespace Bud.Configuration {
  public class InitConfig<T> : IConfigTransform {
    public InitConfig(Key<T> key, Func<IConfigs, T> valueFactory) {
      Key = key;
      ValueFactory = valueFactory;
    }

    public string Key { get; }
    public Func<IConfigs, T> ValueFactory { get; }
    public IConfigDefinition Modify(IConfigDefinition configDefinition) => configDefinition;
    public IConfigDefinition ToConfigDefinition() => new ConfigDefinition<T>(ValueFactory);
  }
}