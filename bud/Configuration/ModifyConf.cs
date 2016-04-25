using System;
using System.Collections.Immutable;
using Bud.V1;

namespace Bud.Configuration {
  public class ModifyConf<T> : ConfBuilder {
    private Func<IConf, T, T> ValueFactory { get; }

    public ModifyConf(Key<T> path, Func<IConf, T, T> valueFactory) : base(path) {
      ValueFactory = valueFactory;
    }

    public override void AddTo(DirectoryDictionary<IConfDefinition> configDefinitions) {
      var oldConfDefinition = configDefinitions.TryGet(Path);
      if (oldConfDefinition.HasValue) {
        var scopedValueFactory = WithScopedValueFactory(oldConfDefinition.Value,
                                                        configDefinitions.CurrentDir);
        configDefinitions.Set(Path, new ConfDefinition<T>(scopedValueFactory));
      } else {
        throw new ConfDefinitionException(Path, typeof(T), $"Could not modify the value of configuration '{Path}'. The configuration has not been initialized yet.");
      }
    }

    private Func<IConf, T> WithScopedValueFactory(IConfDefinition oldConfDefinition,
                                                  IImmutableList<string> scope) {
      return conf => {
        var scopedConf = SubDirConf.ChangeDir(conf, scope);
        return ValueFactory(scopedConf, (T) oldConfDefinition.Value(conf));
      };
    }
  }
}