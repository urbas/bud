using System.Collections.Generic;
using System.Collections.Immutable;
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

    public bool TryGetValue(string key, out T confDefinition)
      => dictionary.TryGetValue(ToFullPath(key, Scope), out confDefinition);

    public ScopedDictionaryBuilder<T> Set(string key, T confDefinition) {
      var fullPath = ToFullPath(key, Scope);
      dictionary[fullPath] = confDefinition;
      return this;
    }

    public bool Contains(string key) => dictionary.ContainsKey(key);

    public ScopedDictionaryBuilder<T> In(string scope)
      => new ScopedDictionaryBuilder<T>(dictionary, Scope.Add(scope));

    public ScopedDictionaryBuilder<T> In(IEnumerable<string> scope)
      => new ScopedDictionaryBuilder<T>(dictionary, Scope.AddRange(scope));

    public ScopedDictionaryBuilder<T> Out()
      => new ScopedDictionaryBuilder<T>(dictionary, Scope.RemoveAt(Scope.Count - 1));

    public T Get(string key) => dictionary[ToFullPath(key, Scope)];

    public ScopedDictionaryBuilder<T> GoToRoot()
      => new ScopedDictionaryBuilder<T>(dictionary, ImmutableList<string>.Empty);
  }
}