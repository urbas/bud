using System;
using System.Collections.Immutable;
using System.IO;
using System.Reactive.Linq;
using Bud.NuGet;
using static System.IO.Directory;
using static System.IO.File;
using static System.IO.Path;
using static Bud.IO.FileUtils;
using static Bud.V1.Api;

namespace Bud.V1 {
  internal static class PackageReferencesProjects {
    internal static Conf CreatePackageReferencesProject(string dir, string projectId)
      => BareProject(dir, projectId)
        .Add(SourcesSupport)
        .AddSourceFile(c => PackagesConfigFile[c])
        .InitValue(AssemblyResolver, new NuGetPackageResolver())
        .Init(PackagesConfigFile, c => Combine(ProjectDir[c], "packages.config"))
        .Init(ResolvedAssemblies, ResolveAssemblies);

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