using System;

namespace Bud.Plugins.NuGet {
  public static class NuGet {
    public static IPlugin Dependency(string packageName, string packageVersion) {
      return Plugin.Create((settings, scope) => settings.Modify(NuGetKeys.NuGetDependencies.In(scope), dependencies => dependencies.Add(new NuGetDependency(packageName, packageVersion))));
    }
  }
}

