using System.IO;
using System.IO.Compression;
using static NUnit.Framework.Assert;

namespace Bud.IO {
  public static class ZipTestUtils {
    public static void IsInZip(string zipFile, params string[] filesInZip) {
      using (var zipStream = File.OpenRead(zipFile)) {
        using (var zipArchive = new ZipArchive(zipStream)) {
          foreach (var fileInZip in filesInZip) {
            IsNotNull(zipArchive.GetEntry(fileInZip),
                      $"The file '{fileInZip}' was not present in '{zipFile}'.");
          }
        }
      }
    }

    public static void IsInZip(string zipFile, string pathInZip, string originalFile) {
      using (var zipStream = File.OpenRead(zipFile)) {
        using (var zipArchive = new ZipArchive(zipStream)) {
          var zipEntry = zipArchive.GetEntry(pathInZip);
          IsNotNull(zipEntry, $"The file '{pathInZip}' was not present in '{zipFile}'.");
          using (var zipEntryStream = zipEntry.Open()) {
            var memoryStream = new MemoryStream();
            zipEntryStream.CopyTo(memoryStream);
            using (var originalFileStream = File.OpenRead(originalFile)) {
              AreEqual(originalFileStream, memoryStream);
            }
          }
        }
      }
    }

    public static void IsNotInZip(string zipFile, string zipEntry) {
      using (var zipStream = File.OpenRead(zipFile)) {
        using (var zipArchive = new ZipArchive(zipStream)) {
          IsNull(zipArchive.GetEntry(zipEntry),
                 $"The file '{zipEntry}' was present in '{zipFile}'.");
        }
      }
    }
  }
}