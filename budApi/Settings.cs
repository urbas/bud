using System;
using System.Linq;
using System.Collections.Immutable;
using Bud.SettingsConstruction;
using Bud.SettingsConstruction.Ops;

namespace Bud {
  public class Settings {
    public static readonly Settings Start = new Settings(ImmutableList.Create<Setting>());

    public readonly ImmutableList<Setting> SettingsList;

    public Settings(ImmutableList<Setting> settings) {
      this.SettingsList = settings;
    }

    public Settings Add(Setting setting) {
      return new Settings(SettingsList.Add(setting));
    }

    public Settings Add(Settings settings) {
      return new Settings(SettingsList.AddRange(settings.SettingsList));
    }
    public Settings Initialize<T>(ConfigKey<T> key, T initialValue) {
      return Add(new InitializeConfig<T>(key, initialValue));
    }

    public Settings Initialize<T>(ConfigKey<T> key, Func<BuildConfiguration, T> initialValue) {
      return Add(new InitializeConfig<T>(key, initialValue));
    }

    public Settings Initialize<T>(TaskKey<T> key, Func<BuildConfiguration, T> task) {
      return Add(new InitializeTask<T>(key, task));
    }

    public Settings EnsureInitialized<T>(ConfigKey<T> key, T initialValue) {
      return Add(new EnsureConfigInitialized<T>(key, initialValue));
    }

    public Settings EnsureInitialized<T>(TaskKey<T> key, Func<BuildConfiguration, T> task) {
      return Add(new EnsureTaskInitialized<T>(key, new TaskDefinition<T>(task)));
    }

    public Settings EnsureInitialized<T>(TaskKey<T> key, Func<T> task) {
      return Add(new EnsureTaskInitialized<T>(key, new TaskDefinition<T>(task)));
    }

    public Settings Modify<T>(ConfigKey<T> key, Func<T, T> modifier) {
      return Add(new ModifyConfig<T>(key, modifier));
    }

    public Settings Modify<T>(TaskKey<T> key, Func<BuildConfiguration, Func<T>, T> modifier) {
      return Add(new ModifyTask<T>(key, modifier));
    }

    public Settings AddDependencies<T>(TaskKey<T> key, params ITaskKey[] dependencies) {
      return Add(new AddDependencies<T>(key, dependencies));
    }

    public ScopedSettings ScopedTo(ISettingKey settingKey) {
      return new ScopedSettings(SettingsList, settingKey);
    }

    public BuildConfiguration ToBuildConfiguration() {
      return BuildConfiguration.ToBuildConfiguration(this);
    }
  }
}