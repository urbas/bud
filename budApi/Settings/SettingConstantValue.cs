using System;
using System.Collections.Immutable;

namespace Bud.Settings {

  internal class SettingConstantValue<T> : SettingValue {
    public readonly T Value;

    public SettingConstantValue(T value) {
      this.Value = value;
    }
  }


}

