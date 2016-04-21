using System.Collections.Generic;
using System.Linq;

namespace Bud.Collections {
  public static class DictionaryUtils {
    public static bool DictionariesEqual<TKey, TValue>(IDictionary<TKey, TValue> dictionary1,
                                                       IDictionary<TKey, TValue> dictionary2) {
      if (dictionary1.Count != dictionary2.Count) {
        return false;
      }
      var equalityComparer = EqualityComparer<TValue>.Default;
      return EqualSizedDictionariesEqual(dictionary1, dictionary2, equalityComparer);
    }

    private static bool EqualSizedDictionariesEqual<TKey, TValue>(IDictionary<TKey, TValue> dictionary1,
                                                                  IDictionary<TKey, TValue> dictionary2,
                                                                  IEqualityComparer<TValue> equalityComparer)
      => dictionary1.All(pair => {
        TValue value;
        return dictionary2.TryGetValue(pair.Key, out value) &&
               equalityComparer.Equals(value, pair.Value);
      });
  }
}