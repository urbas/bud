using System;
using System.Collections.Immutable;

namespace Bud
{
	public class BuildConfiguration
	{
    public readonly ImmutableDictionary<SettingKey, object> SettingKeysToValues;

    public BuildConfiguration(ImmutableDictionary<SettingKey, object> settingKeysToValues) {
      this.SettingKeysToValues = settingKeysToValues;
    }

    public T Evaluate<T>(ConfigKey<T> key) {
      return (T)SettingKeysToValues[key];
    }
	}

}