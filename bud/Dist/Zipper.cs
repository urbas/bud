using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Bud.IO;
using static System.IO.Path;

namespace Bud.Dist {
  public static class Zipper {
    public static void CreateZip(string zipPath, IEnumerable<PackageFile> filesToZip) {
      Directory.CreateDirectory(GetDirectoryName(zipPath));
      using (var distZipStream = File.Open(zipPath, FileMode.Create, FileAccess.Write)) {
        using (var distZip = new ZipArchive(distZipStream, ZipArchiveMode.Create)) {
          foreach (var file in filesToZip) {
            AddToZip(distZip, file);
          }
        }
      }
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