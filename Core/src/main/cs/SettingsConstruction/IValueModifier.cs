using System.Collections.Generic;

namespace Bud.SettingsConstruction {
  public interface IValueModifier<TKey, TValue> {
    void Modify(IDictionary<TKey, TValue> keyValueStore);
    TKey Key { get; }
  }
}