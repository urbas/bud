using System;

namespace Bud.Configuration {
  public class SetConfig<T> : ConfigTransform<T> {
    public SetConfig(Key<T> key, Func<IConf, T> valueFactory) : base(key) {
      ValueFactory = valueFactory;
    }

    public Func<IConf, T> ValueFactory { get; }
    public override ConfigDefinition<T> Modify(ConfigDefinition<T> configDefinition) => ToConfigDefinition();
    public override ConfigDefinition<T> ToConfigDefinition() => new ConfigDefinition<T>(ValueFactory);
  }
}