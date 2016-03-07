using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reactive.Linq;
using Bud.IO;
using Bud.V1;
using static System.IO.Path;
using static Bud.V1.Api;

namespace Bud.Dist {
  internal static class ProjectDistribution {
    internal static readonly Conf DistributionSupport
      = Conf.Empty
            .InitEmpty(FilesToDistribute)
            .Init(DistributionArchive, CreateDistZip)
            .Init(Distribute, BinTrayDistribution.Distribute);

    private static IObservable<string> CreateDistZip(IConf c)
      => FilesToDistribute[c]
        .Select(files => CreateDistZip(c, files));

    private static string CreateDistZip(IConf c, IEnumerable<PackageFile> allFiles) {
      var distZipPath = DistributionArchivePath[c];
      Console.WriteLine($"Creating the distribution package at '{distZipPath}'...");
      Directory.CreateDirectory(GetDirectoryName(distZipPath));
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