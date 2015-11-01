using System;
using System.Collections.Generic;

namespace Bud.Configuration {
  public class ModifyConfig<T> : ConfigTransform {
    public ModifyConfig(Key<T> key, Func<IConf, T, T> valueFactory) : base(key) {
      ValueFactory = valueFactory;
    }

    private Func<IConf, T, T> ValueFactory { get; }

    public override void ApplyIn(IDictionary<string, IConfigDefinition> configDefinitions) {
      IConfigDefinition oldConfigDefinition;
      if (configDefinitions.TryGetValue(Key, out oldConfigDefinition)) {
        configDefinitions[Key] = new ConfigDefinition<T>(conf => ValueFactory(conf, (T) oldConfigDefinition.Value(conf)));
      } else {
        throw new ConfigDefinitionException(Key, typeof(T), $"Could not modify the value of configuration '{Key}'. The configuration has not been initialized yet.");
      }
    }
  }
}