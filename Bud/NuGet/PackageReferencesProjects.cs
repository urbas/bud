using System;
using System.Collections.Immutable;
using System.IO;
using System.Reactive.Linq;
using Bud.V1;
using static System.IO.Directory;
using static System.IO.File;
using static System.IO.Path;
using static Bud.IO.FileUtils;
using static Bud.V1.Api;
using static Bud.V1.ApiImpl;

namespace Bud.NuGet {
  internal static class PackageReferencesProjects {
    private static readonly Conf PackageReferencesProjectSettings = Conf
      .Empty
      .Add(SourcesSupport)
      .AddSourceFile(c => PackagesConfigFile[c])
      .InitValue(AssemblyResolver, new NuGetPackageResolver())
      .Init(PackagesConfigFile, c => Combine(ProjectDir[c], "packages.config"))
      .Init(ResolvedAssemblies, ResolveAssemblies);

    internal static Conf CreatePackageReferencesProject(string dir, string projectId)
      => BareProject(dir, projectId)
        .Add(PackageReferencesProjectSettings);

    internal static IObservable<IImmutableSet<string>> ResolveAssemblies(IConf c)
      => Sources[c].Select(sources => {
        var resolvedAssembliesFile = Combine(TargetDir[c], "resolved_assemblies");
        if (File.Exists(resolvedAssembliesFile) &&
            IsNewerThan(resolvedAssembliesFile, sources)) {
          return ReadAllLines(resolvedAssembliesFile)
            .ToImmutableHashSet();
        }
        var resolvedAssemblies = AssemblyResolver[c]
          .Resolve(sources, ProjectDir[c])
          .ToImmutableHashSet();
        CreateDirectory(TargetDir[c]);
        WriteAllLines(resolvedAssembliesFile, resolvedAssemblies);
        return resolvedAssemblies;
      });
  }
}