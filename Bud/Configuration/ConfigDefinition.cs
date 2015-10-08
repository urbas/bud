using System;

namespace Bud.Configuration {
  public class ConfigDefinition<T> : IConfigDefinition {
    public Func<IConfigs, T> ValueFactory { get; }
    public Type ValueType => typeof(T);
    object IConfigDefinition.Invoke(IConfigs configs) => Invoke(configs);
    public T Invoke(IConfigs configs) => ValueFactory(configs);

    public ConfigDefinition(Func<IConfigs, T> valueFactory) {
      ValueFactory = valueFactory;
    }
  }
}