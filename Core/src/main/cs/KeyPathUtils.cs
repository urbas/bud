using System;

namespace Bud {
  public static class KeyPathUtils {
    public static string ExtractParentPath(string path) {
      var lastIndexOfKeySeparator = path.LastIndexOf(Key.KeySeparator);
      if (lastIndexOfKeySeparator < 0 || IsRootPath(path)) {
        throw new ArgumentException("Cannot extract parent. The path '" + path + "' does not have a parent.");
      }
      if (lastIndexOfKeySeparator == 0) {
        return Key.KeySeparatorAsString;
      }
      return path.Substring(0, lastIndexOfKeySeparator);
    }

    public static string ExtractIdFromPath(string path) {
      if (string.IsNullOrEmpty(path)) {
        throw new ArgumentException("Cannot extract the ID from an empty or null path.");
      }
      if (IsRootPath(path)) {
        return Key.RootId;
      }
      var lastIndexOfKeySeparator = path.LastIndexOf(Key.KeySeparator);
      if (lastIndexOfKeySeparator < 0) {
        return path;
      }
      if (lastIndexOfKeySeparator == path.Length - 1) {
        throw new ArgumentException("Cannot extract the ID from the path '" + path + "'. The path must not end with a path component separator.");
      }
      return path.Substring(lastIndexOfKeySeparator + 1);
    }

    public static bool IsRootPath(string path) {
      return path.Length == 1 && IsAbsolutePath(path);
    }

    public static bool IsAbsolutePath(string path) {
      return path[0] == Key.KeySeparator;
    }

    public static string JoinPath(string pathA, string pathB) {
      if (IsAbsolutePath(pathB)) {
        throw new ArgumentException("Cannot join paths '" + pathA + "' and '" + pathB + "'. The second path must not be absolute.");
      }
      if (string.IsNullOrEmpty(pathA)) {
        return pathB;
      }
      if (string.IsNullOrEmpty(pathB)) {
        return pathA;
      }
      if (IsRootPath(pathA)) {
        return pathA + pathB;
      }
      return pathA + Key.KeySeparator + pathB;
    }
  }
}