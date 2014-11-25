using System;
using System.IO;
using System.Text;

namespace Bud.IO {
  public static class Directories {
    public static string CreateTemporary(string prefix, string suffix) {
      string baseDir = Path.GetTempPath();
      string tempDir;
      Random rng = new Random();
      do {
        tempDir = Path.Combine(baseDir, CreateRandomName(rng, prefix, suffix));
      } while (Directory.Exists(tempDir));
      Directory.CreateDirectory(tempDir);
      return tempDir;
    }

    public static void Copy(string sourceDir, string destDir) {
      if (!Directory.Exists(sourceDir)) {
        throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDir);
      }

      Directory.CreateDirectory(destDir);

      foreach (string file in Directory.GetFiles(sourceDir)) {
        File.Copy(file, Path.Combine(destDir, Path.GetFileName(file)), true);
      }

      foreach (string subdir in Directory.GetDirectories(sourceDir)) {
        Copy(subdir, Path.Combine(destDir, Path.GetFileName(subdir)));
      }
    }

    private static string CreateRandomName(Random rng, string prefix, string suffix) {
      return prefix + Guid.NewGuid().ToString() + suffix;
    }
  }
}

