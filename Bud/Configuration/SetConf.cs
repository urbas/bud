using System;
using System.Collections.Generic;

namespace Bud.Configuration {
  public class SetConf<T> : ConfBuilder {
    public SetConf(Key<T> key, Func<IConf, T> valueFactory) : base(key) {
      ValueFactory = valueFactory;
    }

    public Func<IConf, T> ValueFactory { get; }

    public override void ApplyIn(IDictionary<string, IConfDefinition> configDefinitions)
      => configDefinitions[Key] = new ConfDefinition<T>(ValueFactory);
  }
}