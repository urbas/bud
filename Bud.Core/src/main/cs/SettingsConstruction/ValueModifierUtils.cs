using System.Collections.Generic;
using System.Collections.Immutable;

namespace Bud.SettingsConstruction {
  public static class ValueModifierUtils {
    public static ImmutableDictionary<TKey, TValue> CompileToImmutableDictionary<TKey, TValue>(this IEnumerable<IValueModifier<TKey, TValue>> valueModifiers) {
      var keyValueStore = ImmutableDictionary.CreateBuilder<TKey, TValue>();
      foreach (var modifier in valueModifiers) {
        modifier.Modify(keyValueStore);
      }
      return keyValueStore.ToImmutable();
    }
  }
}