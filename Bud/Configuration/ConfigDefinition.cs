using System;

namespace Bud.Configuration {
  public class ConfigDefinition<T> : IConfigDefinition {
    public Func<IConf, T> ValueFactory { get; }
    public Type ValueType => typeof(T);
    object IConfigDefinition.Invoke(IConf conf) => Invoke(conf);
    public T Invoke(IConf conf) => ValueFactory(conf);

    public ConfigDefinition(Func<IConf, T> valueFactory) {
      ValueFactory = valueFactory;
    }
  }
}