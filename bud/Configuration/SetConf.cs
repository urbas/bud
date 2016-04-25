using System;
using Bud.V1;

namespace Bud.Configuration {
  public class SetConf<T> : ConfBuilder {
    private Func<IConf, T> ValueFactory { get; }

    public SetConf(Key<T> path, Func<IConf, T> valueFactory) : base(path) {
      ValueFactory = valueFactory;
    }

    public override void AddTo(DirectoryDictionary<IConfDefinition> configDefinitions)
      => SetConf.AddTo(configDefinitions, ValueFactory, Path);
  }

  public static class SetConf {
    public static void AddTo<T>(DirectoryDictionary<IConfDefinition> configDefinitions,
                                Func<IConf, T> valueFactory,
                                string key) {
      var dir = configDefinitions.CurrentDir;
      var confDefinition = new ConfDefinition<T>(conf => {
        var scopedConf = SubDirConf.ChangeDir(conf, dir);
        return valueFactory(scopedConf);
      });
      configDefinitions.Set(key, confDefinition);
    }
  }
}