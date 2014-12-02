using System;

namespace Bud {
  public static class DependenciesPlugin {
    public static ScopedSettings WithDependency(this ScopedSettings scopedSettings, string name, string version) {
      return scopedSettings;
    }
  }
}

