using System;
using NUnit.Framework;
using System.IO;
using System.Collections.Generic;

namespace Bud.Test.Assertions {
  public static class FileAssertions {

    public static void AssertFileExists(string absolutePath) {
      var errorMsg = string.Format("The file '{0}' does not exist but was expected to exist.", absolutePath);
      Assert.That(File.Exists(absolutePath), errorMsg);
    }

    public static void AssertFilesExist(IEnumerable<string> absolutePaths) {
      foreach (var absolutePath in absolutePaths) {
        AssertFileExists(absolutePath);
      }
    }

    public static void AssertFileDoesNotExist(string absolutePath) {
      var errorMsg = string.Format("The file '{0}' exists but was expected not to exist.", absolutePath);
      Assert.That(!File.Exists(absolutePath), errorMsg);
    }

    public static void AssertFilesDoNotExist(IEnumerable<string> absolutePaths) {
      foreach (var absolutePath in absolutePaths) {
        AssertFileDoesNotExist(absolutePath);
      }
    }

  }
}

