using System;
using System.Linq;
using System.Collections.Immutable;
using System.Threading.Tasks;
using NuGet;

namespace Bud.Plugins.NuGet {
  public static class NuGet {
    public static IPlugin Dependency(string packageName, string packageVersion) {
      return Plugin.Create((settings, scope) => settings.Modify(NuGetKeys.NuGetDependencies.In(scope), dependencies => dependencies.Add(new NuGetDependency(packageName, packageVersion))));
    }

    public static string GetNuGetRepositoryDir(this EvaluationContext context) {
      return context.Evaluate(NuGetKeys.NuGetRepositoryDir);
    }

    public static ImmutableList<NuGetDependency> GetNuGetDependencies(this EvaluationContext context) {
      var scopesWithNuGetDependencies = context.GetScopesWithNuGetDependencies();
      var nuGetDependencies = scopesWithNuGetDependencies.SelectMany(scope => context.GetNuGetDependencies(scope));
      return ImmutableList.CreateRange(nuGetDependencies);
    }

    public static ImmutableList<NuGetDependency> GetNuGetDependencies(this EvaluationContext context, Scope scope) {
      return context.Evaluate(NuGetKeys.NuGetDependencies.In(scope));
    }

    public static ImmutableList<Scope> GetScopesWithNuGetDependencies(this EvaluationContext context) {
      return context.Evaluate(NuGetKeys.ScopesWithNuGetDependencies);
    }

    public static Task<ImmutableDictionary<string, IPackage>> ResolveNuGetDependencies(this EvaluationContext context) {
      return context.Evaluate(NuGetKeys.ResolveNuGetDependencies);
    }
  }
}

