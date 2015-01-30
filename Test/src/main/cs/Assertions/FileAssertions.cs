using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Bud.Test.Assertions {
  public static class FileAssertions {
    public static void FileExists(string absolutePath) {
      var errorMsg = string.Format("The file '{0}' does not exist but was expected to exist.", absolutePath);
      Assert.That(File.Exists(absolutePath), errorMsg);
    }

    public static void FilesExist(params string[] absolutePaths) {
        FilesExist(absolutePaths as IEnumerable<string>);
    }

    public static void FilesExist(IEnumerable<string> absolutePaths) {
      foreach (var absolutePath in absolutePaths) {
        FileExists(absolutePath);
      }
    }

    public static void FileDoesNotExist(string absolutePath) {
      var errorMsg = string.Format("The file '{0}' exists but was expected not to exist.", absolutePath);
      Assert.That(!File.Exists(absolutePath), errorMsg);
    }

    public static void FilesDoNotExist(params string[] absolutePaths) {
      FilesDoNotExist(absolutePaths as IEnumerable<string>);
    }

    public static void FilesDoNotExist(IEnumerable<string> absolutePaths) {
      foreach (var absolutePath in absolutePaths) {
        FileDoesNotExist(absolutePath);
      }
    }

    public static void DirectoriesDoNotExist(params string[] paths) {
      DirectoriesDoNotExist(paths as IEnumerable<string>);
    }

    private static void DirectoriesDoNotExist(IEnumerable<string> paths) {
      foreach (var path in paths) {
        DirectoryDoesNotExist(path);
      }
    }

    public static void DirectoryDoesNotExist(string path) {
      var errorMsg = string.Format("The directory '{0}' exists but was expected not to exist.", path);
      Assert.That(!Directory.Exists(path), errorMsg);
    }

    public static void DirectoryExists(string path) {
      var errorMsg = string.Format("The directory '{0}' does not exists but was expected to exist.", path);
      Assert.That(Directory.Exists(path), errorMsg);
    }

    public static void DirectoriesExist(params string[] paths) {
      DirectoriesExist(paths as IEnumerable<string>);
    }

    private static void DirectoriesExist(IEnumerable<string> paths) {
      foreach (var path in paths) {
        DirectoryExists(path);
      }
    }
  }
}