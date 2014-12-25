using System;
using System.Linq;
using Bud.Plugins;
using System.Collections.Immutable;
using Bud.Plugins.Projects;
using System.IO;
using System.Threading.Tasks;
using Bud.Commander;
using Bud.Plugins.Dependencies;

namespace Bud.Plugins.NuGet {
  public static class NuGetUtils {
    public static Settings NuGet(this Settings dependent, string packageName, string packageVersion) {
      return dependent.Modify(NuGetKeys.NuGetDependencies.In(dependent.CurrentScope), dependencies => dependencies.Add(new NuGetDependency(packageName, packageVersion)));
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
  }
}

