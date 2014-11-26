using System;
using Bud.Test.Util;
using NUnit.Framework;
using System.IO;

namespace Bud.Test.Assertions {
  public static class TemporaryDirectoryAssertions {

    public static void AssertFileExists(this TemporaryDirectory tempDir, string relativePath) {
      FileAssertions.AssertFileExists(Path.Combine(tempDir.Path, relativePath));
    }

    public static void AssertOutputFileExists(this TemporaryDirectory tempDir, string relativePathInOutputDir) {
      var absolutePath = Path.Combine(BudPaths.GetOutputDirectory(tempDir.Path), relativePathInOutputDir);
      FileAssertions.AssertFileExists(absolutePath);
    }

    public static void AssertOutputFileDoesNotExist(this TemporaryDirectory tempDir, string relativePathInOutputDir) {
      var absolutePath = Path.Combine(BudPaths.GetOutputDirectory(tempDir.Path), relativePathInOutputDir);
      FileAssertions.AssertFileDoesNotExist(absolutePath);
    }
  }
}

