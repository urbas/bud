using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Bud.Cs;
using Bud.IO;
using Bud.NuGet;
using Bud.Util;

namespace Bud.V1 {
  /// <summary>
  ///   Defines the core concepts of every build in Bud.
  ///   <para>
  ///     Every build has an ID and a directory.
  ///   </para>
  ///   <para>
  ///     In addition, every build has three observable streams: input, build, and output.
  ///     The input is piped (unmodified) through to the build and then frurther
  ///     through to output.
  ///   </para>
  ///   <para>
  ///     The build is defined entirely through keys defined in this class. For example,
  ///     the input, build, and output are defined with keys <see cref="Builds.Input" />,
  ///     <see cref="Builds.Build" />, and <see cref="Conf.Out" />. One can customise these through
  ///     the <see cref="Conf" /> API (such as the <see cref="Conf.Modify{T}" /> method).
  ///   </para>
  /// </summary>
  public static class Api {
    #region Publishing Support

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
      => Basic.Project(projectId, projectDir, baseDir)
              .Add(NuGetPublishing.NuGetPublishingSupport);

    #endregion

    #region Distribution Support

    /// <summary>
    ///   Returns a list of files to package. These file will end up in
    ///   the archive at <see cref="DistributionArchivePath" /> produced by
    ///   <see cref="DistributionArchive" />.
    /// </summary>
    public static readonly Key<IObservable<IImmutableList<PackageFile>>> FilesToDistribute = nameof(FilesToDistribute);

    /// <summary>
    ///   The path where <see cref="DistributionArchive" /> should place the archive.
    /// </summary>
    public static readonly Key<string> DistributionArchivePath = nameof(DistributionArchivePath);

    /// <summary>
    ///   Creates an archive that contains all that is needed for the
    ///   distribution of the project. It returns the path to the created
    ///   archive.
    /// </summary>
    public static readonly Key<IObservable<string>> DistributionArchive = nameof(DistributionArchive);

    /// <summary>
    ///   Pushes the project to a distribution channel. The default implementation places
    ///   the <see cref="DistributionArchive" /> into BinTray, uploads a Chocolatey
    ///   package to the Chocolatey page, and returns <c>true</c> if the operation
    ///   succeeded.
    /// </summary>
    public static readonly Key<IObservable<bool>> Distribute = nameof(Distribute);

    /// <summary>
    ///   Provides the <see cref="DistributionArchive" /> task, which produces
    ///   a distributable archive. The default implementation of the distribution
    ///   produces a ZIP archive in the <see cref="DistributionArchivePath" />. This path
    ///   is not set by default, you have to set it to the desired value.
    ///   <para>
    ///     Add files to the <see cref="FilesToDistribute" /> list in order to include them
    ///     in the produced ZIP archive.
    ///   </para>
    /// </summary>
    public static Conf DistributionSupport => Dist.ProjectDistribution.DistributionSupport;

    #endregion

    #region Package Reference Projects

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
    public static Conf PackageReferencesProject(string projectId,
                                                Option<string> projectDir = default(Option<string>),
                                                Option<string> baseDir = default(Option<string>))
      => PackageReferencesProjects.CreatePackageReferencesProject(projectId, projectDir, baseDir);

    #endregion
  }
}