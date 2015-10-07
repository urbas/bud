using System;

namespace Bud.Configuration {
  public class ConfigDefinition : IConfigDefinition {
    public Type ValueType { get; }
    public Func<IConfigs, object> ValueFactory { get; }

    public ConfigDefinition(Type valueType, Func<IConfigs, object> valueFactory) {
      ValueType = valueType;
      ValueFactory = valueFactory;
    }

    public object Invoke(IConfigs configs) => ValueFactory(configs);
  }

  public class ConfigDefinition<T> : IConfigDefinition {
    public Func<IConfigs, T> ValueFactory { get; }
    public Type ValueType => typeof(T);
    public object Invoke(IConfigs configs) => ValueFactory(configs);

    public ConfigDefinition(Func<IConfigs, T> valueFactory) {
      ValueFactory = valueFactory;
    }
  }
}