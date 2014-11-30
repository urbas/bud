using System;
using System.IO;

namespace Bud {
  public static class BuildPlugin {
    /// <summary>
    /// The global build task.
    /// </summary>
    public static readonly TaskKey<Unit> Build = new TaskKey<Unit>("Build");
    public static readonly TaskKey<Unit> Clean = new TaskKey<Unit>("Clean");

    public static Settings AddBuildSupport(this Settings existingSettings) {
      return existingSettings
        .EnsureInitialized(Clean, () => Unit.Instance)
        .EnsureInitialized(Build, () => Unit.Instance);
    }
  }
}

