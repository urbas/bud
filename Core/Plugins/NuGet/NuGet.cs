using System;
using System.Linq;
using System.Collections.Immutable;
using System.Threading.Tasks;
using NuGet;
using System.Collections.Generic;
using System.IO;
using Bud.Plugins.Build;

namespace Bud.Plugins.NuGet {
  public static class NuGet {
    public const string FetchedPackagesFileName = "nuGetPackages";

    public static IPlugin Dependency(string packageName, string packageVersion) {
      return Plugin.Create((settings, key) => settings.Modify(NuGetKeys.NuGetDependencies.In(key), dependencies => dependencies.Add(new NuGetDependency(packageName, packageVersion))));
    }

    public static string GetNuGetRepositoryDir(this IEvaluationContext context) {
      return context.Evaluate(NuGetKeys.NuGetRepositoryDir);
    }

    public static IDictionary<Key, ImmutableList<NuGetDependency>> GetNuGetDependencies(this IEvaluationContext context) {
      var keysWithNuGetDependencies = context.GetKeysWithNuGetDependencies();
      var nuGetDependencies = keysWithNuGetDependencies.ToDictionary(key => key, key => context.GetNuGetDependencies(key));
      return nuGetDependencies;
    }

    public static ImmutableList<NuGetDependency> GetNuGetDependencies(this IEvaluationContext context, Key key) {
      return context.Evaluate(NuGetKeys.NuGetDependencies.In(key));
    }

    public static ImmutableList<Key> GetKeysWithNuGetDependencies(this IEvaluationContext context) {
      return context.Evaluate(NuGetKeys.KeysWithNuGetDependencies);
    }

    public static string GetFetchedPackagesFile(this IEvaluationContext context) {
      return Path.Combine(BuildDirs.GetPersistentBuildConfigDir(context), FetchedPackagesFileName);
    }
  }
}

