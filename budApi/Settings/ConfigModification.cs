using System;
using System.Collections.Immutable;

namespace Bud.Settings
{
  internal class ConfigModification<T> : Setting
  {
    public ConfigModification(ConfigKey<T> key, SettingValueModification<T> value) : base(key, value) { }
	}


}

