using System;
using System.Collections.Generic;

namespace Bud {
  public class Setting {
    public readonly SettingValue Value;
    public readonly SettingKey Key;

    public Setting(SettingKey key, SettingValue value) {
      this.Value = value;
      this.Key = key;
    }
  }
}

