using System;
using System.Linq;
using System.Collections.Immutable;
using System.Threading.Tasks;
using NuGet;

namespace Bud.Plugins.NuGet {
  public static class NuGet {
    public static IPlugin Dependency(string packageName, string packageVersion) {
      return Plugin.Create((settings, key) => settings.Modify(NuGetKeys.NuGetDependencies.In(key), dependencies => dependencies.Add(new NuGetDependency(packageName, packageVersion))));
    }

    public static string GetNuGetRepositoryDir(this EvaluationContext context) {
      return context.Evaluate(NuGetKeys.NuGetRepositoryDir);
    }

    public static ImmutableList<NuGetDependency> GetNuGetDependencies(this EvaluationContext context) {
      var keysWithNuGetDependencies = context.GetKeysWithNuGetDependencies();
      var nuGetDependencies = keysWithNuGetDependencies.SelectMany(key => context.GetNuGetDependencies(key));
      return ImmutableList.CreateRange(nuGetDependencies);
    }

    public static ImmutableList<NuGetDependency> GetNuGetDependencies(this EvaluationContext context, Key key) {
      return context.Evaluate(NuGetKeys.NuGetDependencies.In(key));
    }

    public static ImmutableList<Key> GetKeysWithNuGetDependencies(this EvaluationContext context) {
      return context.Evaluate(NuGetKeys.KeysWithNuGetDependencies);
    }

    public static Task<ImmutableDictionary<string, IPackage>> ResolveNuGetDependencies(this EvaluationContext context) {
      return context.Evaluate(NuGetKeys.ResolveNuGetDependencies);
    }
  }
}

