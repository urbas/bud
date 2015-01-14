namespace Bud.IO {
  public class Paths {
    public static string ToAgnosticPath(string path) {
      if (string.IsNullOrEmpty(path)) {
        return path;
      }
      return path.Replace('\\', '/');
    }
  }
}