using System;
using System.Collections.Generic;

namespace Bud.Configuration {
  public class InitConfig<T> : ConfBuilder {
    public InitConfig(Key<T> key, Func<IConf, T> valueFactory) : base(key) {
      ValueFactory = valueFactory;
    }

    public Func<IConf, T> ValueFactory { get; }

    public override void ApplyIn(IDictionary<string, IConfigDefinition> configDefinitions) {
      if (!configDefinitions.ContainsKey(Key)) {
        configDefinitions.Add(Key, new ConfigDefinition<T>(ValueFactory));
      }
    }
  }
}