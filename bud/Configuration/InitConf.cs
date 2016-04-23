using System;
using Bud.V1;

namespace Bud.Configuration {
  public class InitConf<T> : ConfBuilder {
    public Func<IConf, T> ValueFactory { get; }

    public InitConf(Key<T> key, Func<IConf, T> valueFactory) : base(key) {
      ValueFactory = valueFactory;
    }

    public override void ApplyIn(DirectoryDictionary<IConfDefinition> configDefinitions) {
      if (!configDefinitions.TryGet(Key).HasValue) {
        SetConf.DefineConfIn(configDefinitions, ValueFactory, Key);
      }
    }
  }
}