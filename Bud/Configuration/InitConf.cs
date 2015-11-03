using System;
using System.Collections.Generic;

namespace Bud.Configuration {
  public class InitConf<T> : ConfBuilder {
    public InitConf(Key<T> key, Func<IConf, T> valueFactory) : base(key) {
      ValueFactory = valueFactory;
    }

    public Func<IConf, T> ValueFactory { get; }

    public override void ApplyIn(IDictionary<string, IConfDefinition> configDefinitions) {
      if (!configDefinitions.ContainsKey(Key)) {
        configDefinitions.Add(Key, new ConfDefinition<T>(ValueFactory));
      }
    }
  }
}