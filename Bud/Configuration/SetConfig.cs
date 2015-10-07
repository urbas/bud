using System;

namespace Bud.Configuration {
  public class SetConfig<T> : IConfigTransform {
    public SetConfig(Key<T> key, Func<IConfigs, T> valueFactory) {
      Key = key;
      ValueFactory = valueFactory;
    }

    public Func<IConfigs, T> ValueFactory { get; }
    public string Key { get; }
    public IConfigDefinition Modify(IConfigDefinition configDefinition) => ToConfigDefinition();
    public IConfigDefinition ToConfigDefinition() => new ConfigDefinition<T>(ValueFactory);
  }
}