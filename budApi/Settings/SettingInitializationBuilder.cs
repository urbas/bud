using System;
using System.Collections.Immutable;

namespace Bud.Settings
{

	public class SettingInitializationBuilder
	{
    public readonly ImmutableList<Setting> ExistingSettings;
    public readonly SettingKey Key;

    public SettingInitializationBuilder(ImmutableList<Setting> existingSettings, SettingKey key) {
      this.Key = key;
      this.ExistingSettings = existingSettings;
    }

    public ImmutableList<Setting> WithValue(ImmutableHashSet<string> value) {
      return ExistingSettings.Add(new SettingInitialization(Key, new SettingConstantValue(value)));
    }
	}

}

