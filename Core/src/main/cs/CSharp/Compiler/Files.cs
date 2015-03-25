using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bud.IO {
  public static class Files {
    public static bool AreFilesNewer(IEnumerable<string> sourceFiles, string targetFile) {
      var lastWriteTime = File.GetLastWriteTime(targetFile);
      return sourceFiles.Any(sourceFile => IsFileNewer(sourceFile, lastWriteTime));
    }

    public static bool IsFileNewer(string sourceFile, DateTime dateTime) {
      return File.GetLastWriteTime(sourceFile) >= dateTime;
    }

    public static void CopyFile(string referencedAssembly, string targetDir) {
      File.Copy(referencedAssembly, Path.Combine(targetDir, Path.GetFileName(referencedAssembly)), true);
    }
  }
}