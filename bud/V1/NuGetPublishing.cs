using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Bud.IO;
using Bud.NuGet;
using Bud.Util;
using static Bud.Util.Option;
using static Bud.V1.Basic;

namespace Bud.V1 {
  public static class NuGetPublishing {
    public const string PackageOutputDirName = "nuget-package";

    /// <summary>
    ///   The home page of the project. For example, https://github.com/urbas/bud.
    /// </summary>
    public static readonly Key<Option<string>> ProjectUrl = nameof(ProjectUrl);

    /// <summary>
    ///   Publishes a project to a distribution endpoint. For example,
    ///   projects like <see cref="Cs.CsLib" /> are published
    ///   to a NuGet repository.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Returns a stream of booleans which indicate whether the publication
    ///     was successful.
    ///   </para>
    /// </remarks>
    public static readonly Key<IObservable<bool>> Publish = nameof(Publish);

    public static Key<IPublisher> Publisher = nameof(Publisher);

    /// <summary>
    ///   The repository URL to which to publish the package. The default
    ///   is NuGet's main repository.
    /// </summary>
    public static Key<Option<string>> PublishUrl = nameof(PublishUrl);

    /// <summary>
    ///   The API key to use when publishing with NuGet.
    /// </summary>
    public static Key<Option<string>> PublishApiKey = nameof(PublishApiKey);

    /// <summary>
    ///   Creates a package and returns the path to the created package.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Returns a stream of paths to the created package.
    ///   </para>
    /// </remarks>
    public static readonly Key<IObservable<string>> Package = nameof(Package);

    public static Key<IPackager> Packager = nameof(Packager);
    public static Key<string> PackageOutputDir = nameof(PackageOutputDir);
    public static Key<IObservable<IEnumerable<PackageFile>>> PackageFiles = nameof(PackageFiles);
    public static Key<NuGetPackageMetadata> PackageMetadata = nameof(PackageMetadata);

    internal static Conf NuGetPublishingConf
      = Conf.Empty
            .Init(PackageMetadata, DefaultPackageMetadata)
            .Init(PackageOutputDir, c => Path.Combine(BuildDir[c], PackageOutputDirName))
            .Init(ProjectUrl, None<string>())
            .Init(Publish, DefaultPublish)
            .Init(PublishUrl, None<string>())
            .Init(PublishApiKey, None<string>())
            .Init(Packager, new NuGetPackager())
            .Init(Publisher, new NuGetPublisher())
            .Init(Package, DefaultPackage)
            .Init(PackageFiles, DefaultPackageFiles);

    /// <param name="projectId">see <see cref="Basic.ProjectId" />.</param>
    /// <param name="projectDir">
    ///   This is the directory in which all sources of this project will live.
    ///   <para>
    ///     If none given, the <see cref="Basic.ProjectDir" /> will be <see cref="Basic.BaseDir" /> appended with the
    ///     <see cref="Basic.ProjectId" />.
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
    public static Conf NuGetPublishingProject(string projectId,
                                              Option<string> projectDir = default(Option<string>),
                                              Option<string> baseDir = default(Option<string>))
      => Project(projectId, projectDir, baseDir)
        .Add(NuGetPublishingConf);

    private static NuGetPackageMetadata DefaultPackageMetadata(IConf c)
      => new NuGetPackageMetadata(Environment.UserName,
                                  ProjectId[c],
                                  ExtractOptionalFields(c));

    private static IImmutableDictionary<string, string> ExtractOptionalFields(IConf c) {
      var projectUrl = c.TryGet(ProjectUrl).Flatten();
      if (projectUrl.HasValue) {
        return ImmutableDictionary<string, string>.Empty
                                                  .Add("projectUrl", projectUrl.Value);
      }
      return ImmutableDictionary<string, string>.Empty;
    }

    private static IObservable<string> DefaultPackage(IConf c)
      => PackageFiles[c].CombineLatest(GetReferencedPackages(c),
                                       (packageFiles, referencedPackages) => Pack(c, packageFiles, referencedPackages));

    private static IObservable<IImmutableList<PackageReference>> GetReferencedPackages(IConf c)
      => c.TryGet(NuGetReferences.ReferencedPackages)
          .GetOrElse(Observable.Return(ImmutableList<PackageReference>.Empty));

    private static string Pack(IConf c,
                               IEnumerable<PackageFile> packageFiles,
                               IEnumerable<PackageReference> referencedPackages)
      => Packager[c].Pack(
        PackageOutputDir[c],
        BaseDir[c],
        ProjectId[c],
        ProjectVersion[c],
        packageFiles,
        PackageDependencies(c).Concat(referencedPackages.Select(r => new PackageDependency(r.Id, r.Version.ToString()))),
        PackageMetadata[c]);

    private static IObservable<IEnumerable<PackageFile>> DefaultPackageFiles(IConf c)
      => c.TryGet(Builds.Output)
          .GetOrElse(Observable.Return(Enumerable.Empty<string>()))
          .Select(files => files.Select(ToContentFiles));

    private static IObservable<bool> DefaultPublish(IConf c)
      => Package[c].Select(package => Publisher[c].Publish(package,
                                                           PublishUrl[c],
                                                           PublishApiKey[c]));

    private static PackageFile ToContentFiles(string file)
      => new PackageFile(file, $"content/{Path.GetFileName(file)}");

    private static IEnumerable<PackageDependency> PackageDependencies(IConf c)
      => Dependencies[c].Select(dependency => ToPackageDependency(c, dependency));

    private static PackageDependency ToPackageDependency(IConf c, string dependency)
      => new PackageDependency(c.Get(dependency/ProjectId),
                               c.Get(dependency/ProjectVersion));
  }
}