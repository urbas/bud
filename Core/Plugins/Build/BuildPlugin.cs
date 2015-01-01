using System;
using System.IO;
using Bud.Util;

namespace Bud.Plugins.Build {
  public class BuildPlugin : IPlugin {
    public static readonly BuildPlugin Instance = new BuildPlugin();

    private BuildPlugin() {}

    public Settings ApplyTo(Settings settings, Scope scope) {
      return settings
        .InitOrKeep(BuildKeys.Clean, TaskUtils.NoOpTask)
        .InitOrKeep(BuildKeys.Build, TaskUtils.NoOpTask)
        .InitOrKeep(BuildKeys.Build.In(scope), TaskUtils.NoOpTask)
        .AddDependencies(BuildKeys.Build, BuildKeys.Build.In(scope));
    }
  }
}

