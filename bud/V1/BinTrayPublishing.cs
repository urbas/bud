using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.IO.Compression;
using System.Reactive.Linq;
using Bud.Dist;
using Bud.IO;

namespace Bud.V1 {
  public static class BinTrayPublishing {
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
    public static Conf DistributionSupport = Conf
      .Empty
      .InitEmpty(FilesToDistribute)
      .Init(DistributionArchive, CreateDistZip)
      .Init(Distribute, BinTrayDistribution.Distribute);

    private static IObservable<string> CreateDistZip(IConf c)
      => FilesToDistribute[c]
        .Select(files => CreateDistZip(c, files));

    private static string CreateDistZip(IConf c, IEnumerable<PackageFile> allFiles) {
      var distZipPath = DistributionArchivePath[c];
      Console.WriteLine($"Creating the distribution package at '{distZipPath}'...");
      Directory.CreateDirectory(Path.GetDirectoryName(distZipPath));
      using (var distZipStream = File.Open(distZipPath, FileMode.Create, FileAccess.Write)) {
        using (var distZip = new ZipArchive(distZipStream, ZipArchiveMode.Create)) {
          foreach (var file in allFiles) {
            AddToZip(distZip, file);
          }
        }
      }
      Console.WriteLine($"Created the distribution package at '{distZipPath}'.");
      return distZipPath;
    }

    private static void AddToZip(ZipArchive distZip, PackageFile path) {
      var entry = distZip.CreateEntry(path.PathInPackage,
                                      CompressionLevel.Optimal);
      using (var entryStream = entry.Open()) {
        using (var entryFile = File.OpenRead(path.FileToPackage)) {
          entryFile.CopyTo(entryStream);
        }
      }
    }
  }
}