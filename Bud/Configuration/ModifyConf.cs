using System;
using System.Collections.Immutable;
using Bud.V1;

namespace Bud.Configuration {
  public class ModifyConf<T> : ConfBuilder {
    private Func<IConf, T, T> ValueFactory { get; }

    public ModifyConf(Key<T> key, Func<IConf, T, T> valueFactory) : base(key) {
      ValueFactory = valueFactory;
    }

    public override void ApplyIn(ScopedDictionaryBuilder<IConfDefinition> configDefinitions) {
      var oldConfDefinition = configDefinitions.TryGet(Key);
      if (oldConfDefinition.HasValue) {
        var scopedValueFactory = WithScopedValueFactory(oldConfDefinition.Value,
                                                        configDefinitions.Scope);
        configDefinitions.Set(Key, new ConfDefinition<T>(scopedValueFactory));
      } else {
        throw new ConfDefinitionException(Key, typeof(T), $"Could not modify the value of configuration '{Key}'. The configuration has not been initialized yet.");
      }
    }

    private Func<IConf, T> WithScopedValueFactory(IConfDefinition oldConfDefinition,
                                                  ImmutableList<string> scope) {
      return conf => {
        var scopedConf = ScopedConf.MakeScoped(scope, conf);
        return ValueFactory(scopedConf, (T) oldConfDefinition.Value(conf));
      };
    }
  }
}