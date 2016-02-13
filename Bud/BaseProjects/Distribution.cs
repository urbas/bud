using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reactive.Linq;
using Bud.IO;
using Bud.V1;
using static System.IO.Directory;
using static System.IO.Path;
using static Bud.V1.Api;

namespace Bud.BaseProjects {
  internal static class Distribution {
    internal static readonly Conf DistributionSupport
      = Conf.Empty
            .InitEmpty(FilesToDistribute)
            .Init(DistributionArchive, CreateDistZip);

    private static IObservable<string> CreateDistZip(IConf c)
      => FilesToDistribute[c]
        .Select(files => CreateDistZip(c, files));

    private static string CreateDistZip(IConf c, IEnumerable<PackageFile> allFiles) {
      var distZipPath = DistributionArchivePath[c];
      CreateDirectory(GetDirectoryName(distZipPath));
      using (var distZipStream = File.Open(distZipPath, FileMode.Create, FileAccess.Write)) {
        using (var distZip = new ZipArchive(distZipStream, ZipArchiveMode.Create)) {
          foreach (var file in allFiles) {
            AddToZip(distZip, file);
          }
        }
      }
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