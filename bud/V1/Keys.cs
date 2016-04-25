using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bud.Util;

namespace Bud.V1 {
  public static class Keys {
    public static readonly Key Root = new Key("");
    public const char Separator = '/';
    public const string BacktrackPath = "../";
    public const string SeparatorAsString = "/";

    public static Key<T> ToAbsolute<T>(this Key<T> configKey) {
      if (configKey.IsAbsolute) {
        return configKey;
      }
      return "/" + configKey.Id;
    }

    public static Key<T> Relativize<T>(this Key<T> configKey)
      => configKey.IsAbsolute ? configKey.Id.Substring(1) : configKey.Id;

    public static string PrefixWith(string parentKey, string childKey)
      => string.IsNullOrEmpty(childKey) ?
        parentKey :
        parentKey + Separator + childKey;

    public static Conf Set<T>(this Key<T> key, T value) => Conf.Empty.Set(key, value);
    public static Conf Init<T>(this Key<T> key, T value) => Conf.Empty.Init(key, value);
    public static Conf Set<T>(this Key<T> key, Func<IConf, T> value) => Conf.Empty.Set(key, value);
    public static Conf Init<T>(this Key<T> key, Func<IConf, T> value) => Conf.Empty.Init(key, value);
    public static Conf Modify<T>(this Key<T> key, Func<IConf, T, T> value) => Conf.Empty.Modify(key, value);

    public static bool IsAbsolute(string id)
      => !string.IsNullOrEmpty(id) && id[0] == Separator;

    /// <returns>
    ///   the number of leading <c>../</c> substrings contained in <paramref name="key" />.
    /// </returns>
    public static int CountBacktracks(string key) {
      int backtrackCount = 0;
      int backtrackCharCount = BacktrackPath.Length;
      for (int i = 0; i < key.Length; i += backtrackCharCount) {
        if (!Strings.ContainsAt(key, BacktrackPath, i)) {
          break;
        }
        ++backtrackCount;
      }
      return backtrackCount;
    }

    /// <returns>
    ///   a concatenation of the <paramref name="dir" /> collection separated by
    ///   <see cref="SeparatorAsString" />.
    /// </returns>
    public static string DirToString(IEnumerable<string> dir)
      => string.Join(SeparatorAsString, dir);

    /// <returns>
    ///   a string where all the leading backtrack strings are removed
    ///   from <paramref name="key" />.
    /// </returns>
    /// <remarks>
    ///   Given the <paramref name="key"/> <c>../../a/b/c</c> and
    ///  <paramref name="dir"/> <c>{ foo, bar, moo }</c>
    ///   the method will return <c>foo/a/b/c</c>.
    /// </remarks>
    public static string ToFullPath(string key, IImmutableList<string> dir)
      => IsAbsolute(key) ? key.Substring(1) : InterpretFromDir(key, dir);

    /// <remarks>
    ///   Given the <paramref name="key"/> <c>../../a/b/c</c> and
    ///  <paramref name="dir"/> <c>{ foo, bar, moo }</c>
    ///   the method will return <c>foo/a/b/c</c>.
    /// </remarks>
    public static string InterpretFromDir(string key, IImmutableList<string> dir) {
      if (IsAbsolute(key) || dir.Count == 0) {
        return key;
      }
      var backtracks = CountBacktracks(key);
      if (backtracks >= dir.Count) {
        return key.Substring(dir.Count * BacktrackPath.Length);
      }
      var backtrackedScope = dir.Take(dir.Count - backtracks);
      var keyWithBacktracksRemoved = key.Substring(backtracks * BacktrackPath.Length);
      return DirToString(backtrackedScope) + Separator + keyWithBacktracksRemoved;
    }
  }
}