using System;
using NUnit.Framework;
using System.IO;

namespace Bud.Test.Assertions {
  public static class FileAssertions {

    public static void AssertFileExists(string absolutePath) {
      var errorMsg = string.Format("The file '{0}' does not exist but was expected to exist.", absolutePath);
      Assert.That(File.Exists(absolutePath), errorMsg);
    }

    public static void AssertFileDoesNotExist(string absolutePath) {
      var errorMsg = string.Format("The file '{0}' exists but was expected not to exist.", absolutePath);
      Assert.That(!File.Exists(absolutePath), errorMsg);
    }

  }
}

