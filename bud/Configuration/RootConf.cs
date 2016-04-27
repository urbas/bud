using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Bud.Util;
using Bud.V1;
using static Bud.Util.Option;

namespace Bud.Configuration {
  /// <summary>
  ///   A key-value configuration bag where all keys have paths relative
  ///   to the root.
  /// </summary>
  internal class RootConf : IConf {
    /// <summary>
    ///   A dictionary where all keys are paths without backtracks (i.e.: <c>..</c>).
    ///   These paths look like this: <c>foo/bar/zar</c>, <c>a/b/c/d</c>, etc.
    ///   These paths are considered relative to the root.
    /// </summary>
    public IDictionary<string, IConfDefinition> ConfDefinitions { get; }
    private ConfCache Cache { get; }

    public RootConf(IDictionary<string, IConfDefinition> confDefinitions) {
      ConfDefinitions = confDefinitions;
      Cache = new ConfCache();
    }

    public Option<T> TryGet<T>(Key<T> key)
      => Cache.TryGet(key.Relativize(), RawTryGet);

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