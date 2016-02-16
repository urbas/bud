using System.Collections.Generic;
using System.Collections.Immutable;
using Bud.Util;
using static Bud.Util.Option;
using static Bud.V1.Keys;

namespace Bud.Configuration {
  /// <remarks>
  ///   This class is not thread safe.
  /// </remarks>
  public class ScopedDictionaryBuilder<T> {
    private readonly IDictionary<string, T> dictionary;
    public ImmutableList<string> Scope { get; }

    public ScopedDictionaryBuilder(IDictionary<string, T> dictionary, ImmutableList<string> scope) {
      this.dictionary = dictionary;
      Scope = scope;
    }

    public Option<T> TryGet(string key) {
      T value;
      if (dictionary.TryGetValue(ToFullPath(key, Scope), out value)) {
        return Some(value);
      }
      return None<T>();
    }

    public ScopedDictionaryBuilder<T> Set(string key, T value) {
      var fullPath = ToFullPath(key, Scope);
      dictionary[fullPath] = value;
      return this;
    }

    public ScopedDictionaryBuilder<T> In(string scope)
      => new ScopedDictionaryBuilder<T>(dictionary, Scope.Add(scope));

    public ScopedDictionaryBuilder<T> In(IEnumerable<string> scope)
      => new ScopedDictionaryBuilder<T>(dictionary, Scope.AddRange(scope));
  }
}