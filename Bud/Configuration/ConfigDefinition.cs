using System;

namespace Bud.Configuration {
  public class ConfigDefinition<T> : IConfigDefinition {
    public Func<IConf, T> ValueFactory { get; }
    public T Value(IConf conf) => ValueFactory(conf);
    object IConfigDefinition.Value(IConf conf) => Value(conf);

    public ConfigDefinition(Func<IConf, T> valueFactory) {
      ValueFactory = valueFactory;
    }
  }
}