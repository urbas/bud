using System;
using Bud.V1;

namespace Bud.Configuration {
  public class InitConf<T> : ConfBuilder {
    public Func<IConf, T> ValueFactory { get; }

    public InitConf(Key<T> path, Func<IConf, T> valueFactory) : base(path) {
      ValueFactory = valueFactory;
    }

    public override void AddTo(DirectoryDictionary<IConfDefinition> configDefinitions) {
      if (!configDefinitions.TryGet(Path).HasValue) {
        SetConf.DefineConfIn(configDefinitions, ValueFactory, Path);
      }
    }
  }
}