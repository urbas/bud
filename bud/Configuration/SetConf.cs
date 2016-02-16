using System;
using Bud.V1;

namespace Bud.Configuration {
  public class SetConf<T> : ConfBuilder {
    private Func<IConf, T> ValueFactory { get; }

    public SetConf(Key<T> key, Func<IConf, T> valueFactory) : base(key) {
      ValueFactory = valueFactory;
    }

    public override void ApplyIn(ScopedDictionaryBuilder<IConfDefinition> configDefinitions)
      => SetConf.DefineConfIn(configDefinitions, ValueFactory, Key);
  }

  public static class SetConf {
    public static void DefineConfIn<T>(ScopedDictionaryBuilder<IConfDefinition> configDefinitions, Func<IConf, T> valueFactory, string key) {
      var confDefinition = new ConfDefinition<T>(conf => {
        var scopedConf = ScopedConf.MakeScoped(configDefinitions.Scope, conf);
        return valueFactory(scopedConf);
      });
      configDefinitions.Set(key, confDefinition);
    }
  }
}