using System;
using Bud.Test.Util;
using NUnit.Framework;
using System.IO;

namespace Bud {
  public static class TemporaryDirectoryAssertions {
    public static void AssertFileExists(this TemporaryDirectoryCopy tempDir, string relativePath) {
      var expectedPathInTempDir = Path.Combine(tempDir.Path, relativePath);
      var errorMsg = string.Format("The file '{0}' does not exist in the folder '{1}'.", relativePath, tempDir.Path);
      Assert.That(File.Exists(expectedPathInTempDir), errorMsg);
    }

    public static void AssertOutputFileExists(this TemporaryDirectoryCopy tempDir, string relativePathInOutputDir) {
      var relativePathInTempDir = Path.Combine(BudPaths.GetOutputDirectory(tempDir.Path), relativePathInOutputDir);
      AssertFileExists(tempDir, relativePathInTempDir);
    }
  }
}

