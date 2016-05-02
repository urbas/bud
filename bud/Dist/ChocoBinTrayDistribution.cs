using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reactive.Linq;
using Bud.IO;
using Bud.NuGet;
using Bud.V1;

namespace Bud.Dist {
  public static class ChocoBinTrayDistribution {
    /// <summary>
    ///   Returns a list of files to package. These file will end up in
    ///   the archive at <see cref="ZipPath" /> produced by
    ///   <see cref="Zip" />.
    /// </summary>
    public static readonly Key<IObservable<IImmutableList<PackageFile>>> FilesToZip = nameof(FilesToZip);

    /// <summary>
    ///   The path where <see cref="Zip" /> should place the archive.
    /// </summary>
    public static readonly Key<string> ZipPath = nameof(ZipPath);

    /// <summary>
    ///   Creates an archive that contains all that is needed for the
    ///   distribution of the project. It returns the path to the created
    ///   archive.
    /// </summary>
    public static readonly Key<IObservable<string>> Zip = nameof(Zip);

    /// <summary>
    ///   Pushes the <see cref="Zip" /> to a generic BinTray repository.
    ///   This task returns the download URL of the pushed package.
    /// </summary>
    public static readonly Key<IObservable<string>> PushToBinTray = nameof(PushToBinTray);

    /// <summary>
    ///   Pushes a chocolatey installation package that points to a ZIP at a particular URL.
    /// </summary>
    public static readonly Key<IObservable<bool>> PushToChocolatey = nameof(PushToChocolatey);

    internal static readonly Conf ChocolateyDistributionSupport
      = Conf.Empty
            .InitEmpty(FilesToZip)
            .Init(Zip, CreateZip)
            .Init(PushToBinTray, Upload)
            .Init(PushToChocolatey, DefaultPushToChocolatey);


    private static IObservable<string> CreateZip(IConf c)
      => FilesToZip[c].Select(files => CreateZip(c, files));

    public static string CreateZip(IConf c, IEnumerable<PackageFile> filesToZip) {
      var distZipPath = ZipPath[c];
      Console.WriteLine($"Creating the distribution package at '{distZipPath}'...");
      Zipper.CreateZip(distZipPath, filesToZip);
      Console.WriteLine($"Created the distribution package at '{distZipPath}'.");
      return distZipPath;
    }

    private static IObservable<string> Upload(IConf c)
      => Upload(Zip[c],
                Api.ProjectId[c],
                Api.ProjectId[c],
                Environment.UserName,
                Api.ProjectVersion[c]);

    private static IObservable<string> Upload(IObservable<string> observedArchive,
                                              string repositoryId,
                                              string packadeId,
                                              string username,
                                              string packageVersion)
      => observedArchive
        .Select(archive => BinTray.PushToGenericRepo(archive,
                                                     repositoryId,
                                                     packadeId,
                                                     packageVersion,
                                                     username,
                                                     "zip",
                                                     TimeSpan.FromMinutes(15)));


    private static IObservable<bool> DefaultPushToChocolatey(IConf c)
      => DefaultPushToChocolatey(PushToBinTray[c],
                                 Api.ProjectId[c],
                                 Api.ProjectId[c],
                                 Environment.UserName,
                                 Api.ProjectVersion[c],
                                 Api.BuildDir[c],
                                 Api.PackageMetadata[c]);

    private static IObservable<bool> DefaultPushToChocolatey(
      IObservable<string> observedArchiveUrl,
      string repositoryId,
      string packadeId,
      string username,
      string packageVersion,
      string buildDir,
      NuGetPackageMetadata packageMetadata)
      => observedArchiveUrl
        .Select(archiveUrl => Chocolatey.Push(repositoryId, packadeId, packageVersion, archiveUrl, username, buildDir, packageMetadata));
  }
}