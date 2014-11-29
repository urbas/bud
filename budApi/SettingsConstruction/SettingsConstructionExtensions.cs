using System;
using System.Collections.Immutable;
using Bud.SettingsConstruction.Ops;

namespace Bud.SettingsConstruction {
  public static class SettingsConstructionExtensions {
    public static Settings Initialize<T>(this Settings existingSettings, ConfigKey<T> key, T initialValue) {
      return existingSettings.Add(new InitializeConfig<T>(key, initialValue));
    }

    public static Settings EnsureInitialized<T>(this Settings existingSettings, ConfigKey<T> key, T initialValue) {
      return existingSettings.Add(new EnsureConfigInitialized<T>(key, initialValue));
    }

    public static Settings Modify<T>(this Settings existingSettings, ConfigKey<T> key, Func<T, T> modifier) {
      return existingSettings.Add(new ModifyConfig<T>(key, modifier));
    }

    public static Settings EnsureInitialized<T>(this Settings existingSettings, TaskKey<T> key, Func<BuildConfiguration, T> task) {
      return existingSettings.Add(new EnsureTaskInitialized<T>(key, new GenericTaskDefinition<T>(task)));
    }

    public static Settings EnsureInitialized<T>(this Settings existingSettings, TaskKey<T> key, Func<T> task) {
      return existingSettings.Add(new EnsureTaskInitialized<T>(key, new TaskDefinition<T>(task)));
    }

    public static Settings EnsureInitialized<T, TDependency1>(this Settings existingSettings, TaskKey<T> key, IValuedKey<TDependency1> dependency1, Func<TDependency1, T> task) {
      return existingSettings.Add(new EnsureTaskInitialized<T>(key, new TaskDefinition<T, TDependency1>(dependency1, task)));
    }

    public static Settings Modify<T>(this Settings existingSettings, TaskKey<T> key, Func<Func<T>, BuildConfiguration, T> modifier) {
      return existingSettings.Add(new ModifyTask<T>(key, modifier));
    }
  }
}

