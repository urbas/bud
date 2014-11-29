using System;
using Bud.SettingsConstruction.Ops;
using Bud.SettingsConstruction;

namespace Bud {
  public static class BuildPlugin {
    public static readonly TaskKey<Unit> Build = new TaskKey<Unit>("Build");
    public static readonly TaskKey<Unit> Clean = new TaskKey<Unit>("Clean");

    public static Settings AddBuildSupport(this Settings existingSettings) {
      return existingSettings
        .EnsureInitialized(Clean, () => Unit.Instance)
        .EnsureInitialized(Build, () => Unit.Instance);
    }
  }
}

