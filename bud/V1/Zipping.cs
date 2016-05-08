using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reactive.Linq;
using Bud.IO;
using Bud.Util;
using static Bud.V1.Basic;
using static Bud.V1.Builds;

namespace Bud.V1 {
  public class Zipping {
    /// <summary>
    ///   A list of files to put into the zip archive. These file will end up in
    ///   the zip archive at <see cref="ZipPath" /> produced by <see cref="Zip" />.
    /// </summary>
    public static readonly Key<IObservable<IImmutableList<PackageFile>>> FilesToZip = nameof(FilesToZip);

    /// <summary>
    ///   The path of the produced zip.
    /// </summary>
    public static readonly Key<string> ZipPath = nameof(ZipPath);

    /// <summary>
    ///   Performs the zipping and return the path of the produced zip archime every time it performs the zip.
    /// </summary>
    public static Key<IObservable<string>> Zip = nameof(Zip);

    public static Conf ZipProject(string projectId,
                                  Option<string> projectDir = default(Option<string>),
                                  Option<string> baseDir = default(Option<string>))
      => Project(projectId, projectDir, baseDir)
        .Init(ZipPath, c => Path.Combine(BuildDir[c], $"{ProjectId[c]}.zip"))
        .Init(Zip, DefaultZipping)
        .InitEmpty(FilesToZip);

    private static IObservable<string> DefaultZipping(IConf c) {
      var zipPath = ZipPath[c];
      CreateZipArchive(zipPath, Enumerable.Empty<PackageFile>());
      return FilesToZip[c].Select(filesToZip => CreateZipArchive(zipPath, filesToZip));
    }

    public static string CreateZipArchive(string zipPath, IEnumerable<PackageFile> filesToPackage) {
      Directory.CreateDirectory(Path.GetDirectoryName(zipPath));
      using (var distZipStream = File.Open(zipPath, FileMode.Create, FileAccess.Write)) {
        using (var distZip = new ZipArchive(distZipStream, ZipArchiveMode.Create)) {
          foreach (var file in filesToPackage) {
            AddToZip(distZip, file);
          }
        }
      }
      return zipPath;
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

    private static IImmutableList<PackageFile> ToPackageFiles(IEnumerable<string> absoluteFilePaths,
                                                              Uri projectDir)
      => absoluteFilePaths
        .Select(file => ToPackageFile(projectDir, file))
        .ToImmutableList();

    private static PackageFile ToPackageFile(Uri baseDir, string file)
      => new PackageFile(file, baseDir.MakeRelativeUri(new Uri(file)).ToString());
  }
}