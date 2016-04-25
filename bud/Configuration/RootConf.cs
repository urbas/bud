using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Bud.Util;
using Bud.V1;
using static Bud.Util.Option;

namespace Bud.Configuration {
  internal class RootConf : IConf {
    public IDictionary<string, IConfDefinition> ConfDefinitions { get; }
    private CachingConf CachingConf { get; }

    public RootConf(IDictionary<string, IConfDefinition> confDefinitions) {
      ConfDefinitions = confDefinitions;
      CachingConf = new CachingConf();
    }

    public Option<T> TryGet<T>(Key<T> key)
      => CachingConf.TryGet(key.Relativize(), RawTryGet);

    private Option<T> RawTryGet<T>(Key<T> key) {
      IConfDefinition confDefinition;
      if (ConfDefinitions.TryGetValue(key, out confDefinition)) {
        object value;
        try {
          value = confDefinition.Value(this);
        } catch (Exception e) {
          var cex = e as ConfAccessException;
          if (cex != null) {
            throw new ConfAccessException(cex.ReferencePath.Add(key), cex.InnerException);
          }
          throw new ConfAccessException(ImmutableList.Create(key.Id), e);
        }
        return Some((T) value);
      }
      return None<T>();
    }
  }
}