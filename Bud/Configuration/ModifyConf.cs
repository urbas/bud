using System;
using System.Collections.Generic;

namespace Bud.Configuration {
  public class ModifyConf<T> : ConfBuilder {
    public ModifyConf(Key<T> key, Func<IConf, T, T> valueFactory) : base(key) {
      ValueFactory = valueFactory;
    }

    private Func<IConf, T, T> ValueFactory { get; }

    public override void ApplyIn(IDictionary<string, IConfDefinition> configDefinitions) {
      IConfDefinition oldConfDefinition;
      if (configDefinitions.TryGetValue(Key, out oldConfDefinition)) {
        configDefinitions[Key] = new ConfDefinition<T>(conf => ValueFactory(conf, (T) oldConfDefinition.Value(conf)));
      } else {
        throw new ConfigDefinitionException(Key, typeof(T), $"Could not modify the value of configuration '{Key}'. The configuration has not been initialized yet.");
      }
    }
  }
}