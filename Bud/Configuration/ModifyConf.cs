using System;
using System.Collections.Immutable;

namespace Bud.Configuration {
  public class ModifyConf<T> : ConfBuilder {
    private Func<IConf, T, T> ValueFactory { get; }

    public ModifyConf(Key<T> key, Func<IConf, T, T> valueFactory) : base(key) {
      ValueFactory = valueFactory;
    }

    public override void ApplyIn(ScopedDictionaryBuilder<IConfDefinition> configDefinitions) {
      IConfDefinition oldConfDefinition;
      if (configDefinitions.TryGetValue(Key, out oldConfDefinition)) {
        var scopedValueFactory = WithScopedValueFactory(oldConfDefinition, configDefinitions.Scope);
        configDefinitions.Set(Key, new ConfDefinition<T>(scopedValueFactory, configDefinitions.Scope));
      } else {
        throw new ConfigDefinitionException(Key, typeof(T), $"Could not modify the value of configuration '{Key}'. The configuration has not been initialized yet.");
      }
    }

    private Func<IConf, T> WithScopedValueFactory(IConfDefinition oldConfDefinition,
                                                  ImmutableList<string> scope) {
      return conf => {
        var scopedConf = SubscopingConf.MakeScoped(scope, conf);
        return ValueFactory(scopedConf, (T) oldConfDefinition.Value(conf));
      };
    }
  }
}