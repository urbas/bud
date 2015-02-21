using System;
using System.Collections.Immutable;

namespace Bud {
  public static class KeyPathUtils
  {
    private static readonly char[] KeySplitter = { Key.KeySeparator };

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
      if (String.IsNullOrEmpty(path)) {
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
      if (String.IsNullOrEmpty(pathA)) {
        return pathB;
      }
      if (String.IsNullOrEmpty(pathB)) {
        return pathA;
      }
      if (IsRootPath(pathA)) {
        return pathA + pathB;
      }
      return pathA + Key.KeySeparator + pathB;
    }

    public static ImmutableList<string> ToPathComponents(string path) {
      if (String.IsNullOrEmpty(path)) {
        throw new ArgumentException("Could not parse an empty string. An empty string is not a valid key.");
      }
      var keyIdChain = path.Split(KeySplitter, StringSplitOptions.RemoveEmptyEntries);
      var parsedPath = ImmutableList.CreateBuilder<string>();
      if (path[0] == Key.KeySeparator) {
        parsedPath.Add(Key.RootId);
      }
      for (int index = 0; index < keyIdChain.Length; index++) {
        parsedPath.Add(keyIdChain[index]);
      }
      var pathComponents = parsedPath.ToImmutable();
      return pathComponents;
    }
  }
}