namespace Bud.IO {
  public static class Paths {
    public static string ToAgnosticPath(string path) => string.IsNullOrEmpty(path) ? path : path.Replace('\\', '/');

    public static string ToWindowsPath(string path) => string.IsNullOrEmpty(path) ? path : path.Replace('/', '\\');
  }
}