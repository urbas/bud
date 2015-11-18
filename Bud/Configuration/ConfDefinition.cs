using System;
using System.Collections.Immutable;
using static Bud.Configuration.SubscopingConf;

namespace Bud.Configuration {
  public class ConfDefinition<T> : IConfDefinition {
    public Func<IConf, T> ValueFactory { get; }
    public T Value(IConf conf) => ValueFactory(conf);
    object IConfDefinition.Value(IConf conf) => Value(conf);

    public ConfDefinition(Func<IConf, T> valueFactory,
                          ImmutableList<string> scope) {
      ValueFactory = valueFactory;
    }
  }
}