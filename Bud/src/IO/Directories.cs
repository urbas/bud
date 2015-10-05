using System;
using System.IO;

namespace Bud.IO {
  public static class Directories {
    public static string CreateTemporary(string prefix, string suffix) {
      string baseDir = Path.GetTempPath();
      string tempDir;
      do {
        tempDir = Path.Combine(baseDir, CreateRandomName(prefix, suffix));
      } while (Directory.Exists(tempDir));
      Directory.CreateDirectory(tempDir);
      return tempDir;
    }

    public static void Copy(string sourceDir, string destDir) {
      if (!Directory.Exists(sourceDir)) {
        throw new DirectoryNotFoundException($"Source directory does not exist or could not be found: {sourceDir}");
      }

      Directory.CreateDirectory(destDir);

      foreach (string file in Directory.EnumerateFiles(sourceDir)) {
        File.Copy(file, Path.Combine(destDir, Path.GetFileName(file)), true);
      }

      foreach (string subdir in Directory.EnumerateDirectories(sourceDir)) {
        Copy(subdir, Path.Combine(destDir, Path.GetFileName(subdir)));
      }
    }

    private static string CreateRandomName(string prefix, string suffix) {
      return prefix + Guid.NewGuid() + suffix;
    }
  }
}