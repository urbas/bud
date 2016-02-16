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

    /// <returns>
    ///   A filter that returns <c>true</c> iff the given file is
    ///   in any of the given directories.
    /// </returns>
    public static Func<string, bool> InAnyDirFilter(IEnumerable<string> dirs) {
      var unixDirPaths = dirs.Select(ToSlashedPath).ToList();
      return file => {
        var slashedPath = ToSlashedPath(file);
        return unixDirPaths.Any(dir => IsSlashedPathInDir(slashedPath, dir));
      };
    }

    /// <returns>
    ///   <c>true</c> iff <paramref name="path" /> is a subpath of <paramref name="dir" />.
    /// </returns>
    /// <remarks>
    ///   parameters <paramref name="path" /> and <paramref name="dir" /> must use slashes
    ///   as directory separators (<see cref="ToSlashedPath" />).
    /// </remarks>
    private static bool IsSlashedPathInDir(string path, string dir) {
      if (path.StartsWith(dir)) {
        return path.Length == dir.Length ||
               path[dir.Length] == '/';
      }
      return dir.StartsWith(path) &&
             dir[path.Length] == '/';
    }

    /// <returns>
    ///   a path that uses strictly slashes as directory separators.
    /// </returns>
    private static string ToSlashedPath(string path)
      => path.Replace(Path.DirectorySeparatorChar, '/');
  }
}