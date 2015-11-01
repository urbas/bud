using System;
using System.Collections.Generic;

namespace Bud.Configuration {
  public class SetConfig<T> : ConfigTransform {
    public SetConfig(Key<T> key, Func<IConf, T> valueFactory) : base(key) {
      ValueFactory = valueFactory;
    }

    public Func<IConf, T> ValueFactory { get; }

    public override void ApplyIn(IDictionary<string, IConfigDefinition> configDefinitions)
      => configDefinitions[Key] = new ConfigDefinition<T>(ValueFactory);
  }
}