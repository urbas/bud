using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Bud.IO;
using Bud.NuGet;
using Bud.Util;
using static Bud.V1.Basic;
using static Bud.V1.Builds;

namespace Bud.V1 {
  public static class NuGetReferences {
    /// <summary>
    ///   The path to the <c>packages.config</c> file. By default, it is placed directly
    ///   under the <see cref="Basic.ProjectDir" />.
    /// </summary>
    public static Key<string> PackagesConfigFile = nameof(PackagesConfigFile);

    /// <summary>
    ///   A list of paths to assemblies. These paths are resolved from NuGet
    ///   package references.
    /// </summary>
    public static Key<IObservable<IImmutableList<PackageReference>>> ReferencedPackages = nameof(ReferencedPackages);

    /// <summary>
    ///   A list of paths to assemblies. These paths are resolved from NuGet
    ///   package references.
    /// </summary>
    public static Key<IObservable<IImmutableSet<string>>> ResolvedAssemblies = nameof(ResolvedAssemblies);

    public static Key<IAssemblyResolver> AssemblyResolver = nameof(AssemblyResolver);

    public static Key<NuGetPackageDownloader> PackageDownloader = nameof(PackageDownloader);

    private static readonly Conf NuGetReferencesConfs = Conf
      .Empty
      .Add(Input, c => PackagesConfigFile[c])
      .Init(AssemblyResolver, new NuGetAssemblyResolver())
      .Init(PackageDownloader, new NuGetPackageDownloader())
      .Init(ReferencedPackages, ReadReferencedPackagesFromSources)
      .Init(PackagesConfigFile, c => Path.Combine(ProjectDir[c], "packages.config"))
      .Init(ResolvedAssemblies, ResolveAssemblies);

    /// <param name="projectId">see <see cref="ProjectId" />.</param>
    /// <param name="projectDir">
    ///   This is the directory in which all sources of this project will live.
    ///   <para>
    ///     If none given, the <see cref="Basic.ProjectDir" /> will be <see cref="Basic.BaseDir" /> appended with the
    ///     <see cref="ProjectId" />.
    ///   </para>
    ///   <para>
    ///     If the given path is relative, then the absolute <see cref="Basic.ProjectDir" /> will
    ///     be resolved from the <see cref="Basic.BaseDir" />. Note that the <paramref name="projectDir" />
    ///     can be empty.
    ///   </para>
    ///   <para>
    ///     If the given path is absolute, the absolute path will be taken verbatim.
    ///   </para>
    /// </param>
    /// <param name="baseDir">
    ///   <para>
    ///     The directory under which all projects should live. By default this is the directory
    ///     where the <c>Build.cs</c> script is located.
    ///   </para>
    ///   <para>
    ///     By default this is where the <see cref="Basic.BuildDir" /> will be located.
    ///   </para>
    /// </param>
    public static Conf PackageReferencesProject(string projectId,
                                                Option<string> projectDir = default(Option<string>),
                                                Option<string> baseDir = default(Option<string>))
      => BuildProject(projectId, projectDir, baseDir)
        .Add(NuGetReferencesConfs);

    private static IObservable<IImmutableList<PackageReference>> ReadReferencedPackagesFromSources(IConf c)
      => Input[c].Select(NuGetPackageReferencesReader.LoadReferences)
                 .Select(ImmutableList.ToImmutableList);

    private static IObservable<IImmutableSet<string>> ResolveAssemblies(IConf c)
      => ReferencedPackages[c].Select(packageReferences => {
        var buildDir = BuildDir[c];
        var resolvedAssembliesFile = Path.Combine(buildDir, "resolved_assemblies");
        Directory.CreateDirectory(buildDir);
        var hash = PackageReference.GetHash(packageReferences);
        var resolvedAssemblies = HashBasedCaching.GetLinesOrCache(
          resolvedAssembliesFile,
          hash,
          () => DownloadAndResolvePackages(c, packageReferences));
        return resolvedAssemblies.ToImmutableHashSet();
      });

    private static IEnumerable<string> DownloadAndResolvePackages(IConf c, IReadOnlyCollection<PackageReference> packageReferences) {
      var packagesDir = Path.Combine(BuildDir[c], "cache");
      Directory.CreateDirectory(packagesDir);
      if (packageReferences.Count == 0) {
        return Enumerable.Empty<string>();
      }
      if (!PackageDownloader[c].DownloadPackages(packageReferences, packagesDir)) {
        throw new Exception($"Could not download packages: {string.Join(", ", packageReferences)}");
      }
      return AssemblyResolver[c]
        .FindAssembly(packageReferences, packagesDir, BuildDir[c]);
    }
  }
}