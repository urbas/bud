using System;
using System.IO;
using Bud.Util;

namespace Bud.Plugins.Build {
  public static class BuildPlugin {

    public static Settings AddBuildSupport(this Settings existingSettings) {
      return existingSettings
        .InitOrKeep(BuildKeys.Clean, TaskUtils.NoOpTask)
        .InitOrKeep(BuildKeys.Build, TaskUtils.NoOpTask);
    }
  }
}

