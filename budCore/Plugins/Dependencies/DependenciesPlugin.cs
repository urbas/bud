using System;

namespace Bud.Plugins.Dependencies {
  public static class DependenciesPlugin {
    public static Settings WithDependency(this Settings settings, string name, string version) {
      return settings;
    }
  }
}

