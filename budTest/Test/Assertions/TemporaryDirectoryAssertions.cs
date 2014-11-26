using System;
using Bud.Test.Util;
using NUnit.Framework;
using System.IO;

namespace Bud.Test.Assertions {
  public static class TemporaryDirectoryAssertions {

    public static void AssertFileExists(this TemporaryDirectoryCopy tempDir, string relativePath) {
      FileAssertions.AssertFileExists(Path.Combine(tempDir.Path, relativePath));
    }

    public static void AssertOutputFileExists(this TemporaryDirectoryCopy tempDir, string relativePathInOutputDir) {
      var absolutePath = Path.Combine(BudPaths.GetOutputDirectory(tempDir.Path), relativePathInOutputDir);
      FileAssertions.AssertFileExists(absolutePath);
    }

    public static void AssertOutputFileDoesNotExist(this TemporaryDirectoryCopy tempDir, string relativePathInOutputDir) {
      var absolutePath = Path.Combine(BudPaths.GetOutputDirectory(tempDir.Path), relativePathInOutputDir);
      FileAssertions.AssertFileDoesNotExist(absolutePath);
    }
  }
}

