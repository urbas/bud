using System;
using System.IO;
using Bud.Util;

namespace Bud.Plugins.Build {
  public class BuildPlugin : IPlugin {
    public static readonly BuildPlugin Instance = new BuildPlugin();

    private BuildPlugin() {}

    public Settings ApplyTo(Settings settings, Scope scope) {
      return settings
        .Init(BuildKeys.Clean, TaskUtils.NoOpTask)
        .Init(BuildKeys.Build, TaskUtils.NoOpTask)
        .Init(BuildKeys.Build.In(scope), TaskUtils.NoOpTask)
        .AddDependencies(BuildKeys.Build, BuildKeys.Build.In(scope));
    }
  }
}

