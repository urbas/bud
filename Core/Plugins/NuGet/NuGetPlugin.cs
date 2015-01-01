using System.Collections.Immutable;
using System.Threading.Tasks;
using NuGet;
using Bud.Plugins.Build;
using System.IO;
using System;

namespace Bud.Plugins.NuGet {
  public class NuGetPlugin : IPlugin {

    public static readonly NuGetPlugin Instance = new NuGetPlugin();

    private NuGetPlugin() {}

    public Settings ApplyTo(Settings settings, Scope scope) {
      return settings
        .Init(NuGetKeys.ScopesWithNuGetDependencies, ImmutableList<Scope>.Empty)
        .Init(NuGetKeys.NuGetRepositoryDir, context => Path.Combine(context.GetBudDir(scope), "nuGetRepository"))
        .Init(NuGetKeys.ResolveNuGetDependencies, ResolveNuGetDependenciesImpl)
        .Init(NuGetKeys.NuGetDependencies.In(scope), ImmutableList<NuGetDependency>.Empty)
        .Modify(NuGetKeys.ScopesWithNuGetDependencies, (context, oldValue) => oldValue.Add(scope));
    }

    public static Task<ImmutableDictionary<string, IPackage>> ResolveNuGetDependenciesImpl(EvaluationContext context) {
      return Task.Run(() => {
        var dependencies = context.GetNuGetDependencies();
        var nuGetRepositoryDir = context.GetNuGetRepositoryDir();
        var resolvedDependencies = ImmutableDictionary.CreateBuilder<string, IPackage>();

        IPackageRepository repo = PackageRepositoryFactory.Default.CreateRepository("http://packages.nuget.org/api/v2");
        PackageManager packageManager = new PackageManager(repo, nuGetRepositoryDir);

        packageManager.PackageInstalled += (object sender, PackageOperationEventArgs e) => {
          resolvedDependencies.Add(e.Package.Id, e.Package);
        };

        foreach (var dependency in dependencies) {
          packageManager.InstallPackage(dependency.PackageName, dependency.PackageVersion);
        }

        return resolvedDependencies.ToImmutable();
      });
    }
  }

}

