using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bud.IO {
  public static class PathUtils {
    /// <returns>
    ///   <c>true</c> iff <paramref name="path" /> is a subpath of <paramref name="dir" />.
    /// </returns>
    public static bool IsPathInDir(string path, string dir) {
      if (path == null) {
        throw new ArgumentNullException(nameof(path));
      }
      if (dir == null) {
        throw new ArgumentNullException(nameof(dir));
      }
      return IsSlashedPathInDir(ToSlashedPath(path),
                                ToSlashedPath(dir));
    }

    public static Func<string, bool> NotInAnyDirFilter(IEnumerable<string> dirs) {
      var unixDirPaths = dirs.Select(ToSlashedPath).ToList();
      return file => !unixDirPaths.Any(dir => IsSlashedPathInDir(ToSlashedPath(file), dir));
    }

    /// <param name="path">
    ///   This path must use slashes as directory separators.
    /// </param>
    /// <param name="dir">
    ///   This path must use slashes as directory separators.
    /// </param>
    /// <returns>
    ///   <c>true</c> iff <paramref name="path" /> is a subpath of <paramref name="dir" />.
    /// </returns>
    private static bool IsSlashedPathInDir(string path, string dir) {
      if (path.StartsWith(dir)) {
        return path.Length == dir.Length ||
               path[dir.Length] == '/';
      }
      return dir.StartsWith(path) &&
             dir[path.Length] == '/';
    }

    private static string ToSlashedPath(string path)
      => path.Replace(Path.DirectorySeparatorChar, '/');
  }
}