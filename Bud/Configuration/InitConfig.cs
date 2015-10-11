using System;

namespace Bud.Configuration {
  public class InitConfig<T> : ConfigTransform<T> {
    public InitConfig(Key<T> key, Func<IConfigs, T> valueFactory) : base(key) {
      ValueFactory = valueFactory;
    }

    public Func<IConfigs, T> ValueFactory { get; }
    public override ConfigDefinition<T> ToConfigDefinition() => new ConfigDefinition<T>(ValueFactory);
  }
}