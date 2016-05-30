using System;
using System.IO;
using Bud;

namespace Samples {
  public static class SampleDir {
    public static TmpDir TmpCopy(string sampleName) {
      TmpDir dir = null;
      try {
        dir = new TmpDir();
        CopyDir(Paths.Sample(sampleName), dir.Path);
      } catch (Exception) {
        dir?.Dispose();
        throw;
      }
      return dir;
    }

    private static void CopyDir(string fromDir, string toDir)
      => Copy(new DirectoryInfo(fromDir), new DirectoryInfo(toDir));

    private static void Copy(DirectoryInfo source, DirectoryInfo target) {
      foreach (var dir in source.GetDirectories()) {
        Copy(dir, target.CreateSubdirectory(dir.Name));
      }
      foreach (var file in source.GetFiles()) {
        file.CopyTo(Path.Combine(target.FullName, file.Name));
      }
    }
  }
}