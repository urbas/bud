using System;
using System.IO;
using Bud.Util;

namespace Bud.Plugins.Build {
  public class BuildPlugin : IPlugin {
    public static readonly BuildPlugin Instance = new BuildPlugin();

    private BuildPlugin() {}

    public Settings ApplyTo(Settings settings, Key key) {
      return settings
        .Init(BuildKeys.Clean, TaskUtils.NoOpTask)
        .Init(BuildKeys.Clean.In(key), TaskUtils.NoOpTask)
        .Init(BuildKeys.Build, TaskUtils.NoOpTask)
        .Init(BuildKeys.Build.In(key), TaskUtils.NoOpTask)
        .Init(BuildKeys.Test, TaskUtils.NoOpTask)
        .Init(BuildKeys.Test.In(key), TaskUtils.NoOpTask)
        .AddDependencies(BuildKeys.Build, BuildKeys.Build.In(key))
        .AddDependencies(BuildKeys.Clean, BuildKeys.Clean.In(key))
        .AddDependencies(BuildKeys.Test, BuildKeys.Test.In(key));
    }
  }
}

