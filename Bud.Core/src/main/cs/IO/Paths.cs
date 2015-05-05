using System;

namespace Bud.IO {
  public static class Paths {
    public static string ToAgnosticPath(string path) => string.IsNullOrEmpty(path) ? path : path.Replace('\\', '/');

    public static string ToWindowsPath(string path) => string.IsNullOrEmpty(path) ? path : path.Replace('/', '\\');

    public static string MakeRelative(string filePath, string referencePath) {
      var fileUri = new Uri(filePath);
      var referenceUri = new Uri(referencePath);
      return referenceUri.MakeRelativeUri(fileUri).ToString();
    }
  }
}