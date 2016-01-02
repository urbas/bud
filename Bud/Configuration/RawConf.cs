using System.Collections.Generic;
using Bud.V1;
using Bud.Util;
using static Bud.Util.Optional;

namespace Bud.Configuration {
  public class RawConf : IConf {
    public IDictionary<string, IConfDefinition> ConfDefinitions { get; }
    private CachingConf CachingConf { get; }

    public RawConf(IDictionary<string, IConfDefinition> confDefinitions) {
      ConfDefinitions = confDefinitions;
      CachingConf = new CachingConf();
    }

    public Optional<T> TryGet<T>(Key<T> key)
      => CachingConf.TryGet(key.Relativize(), RawTryGet);

    private Optional<T> RawTryGet<T>(Key<T> key) {
      IConfDefinition confDefinition;
      if (ConfDefinitions.TryGetValue(key, out confDefinition)) {
        return Some((T) confDefinition.Value(this));
      }
      return None<T>();
    }
  }
}