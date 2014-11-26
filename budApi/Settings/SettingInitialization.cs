using System;
using System.Collections.Immutable;

namespace Bud.Settings
{
  internal class SettingInitialization : Setting
  {
    private readonly SettingValue value;
    private readonly SettingKey key;

    public SettingInitialization(SettingKey key, SettingValue value) {
      this.value = value;
      this.key = key;
    }


    public SettingKey Key {
      get {
        return key;
      }
    }

    public SettingValue Value {
      get {
        return value;
      }
    }

	}

}

