using System;
using System.Collections.Generic;
using System.Linq;
using Bud.Configuration;
using Bud.Util;

namespace Bud {
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

    public static Conf SetValue<T>(this Key<T> key, T value) => Conf.Empty.SetValue(key, value);
    public static Conf InitValue<T>(this Key<T> key, T value) => Conf.Empty.InitValue(key, value);
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
    ///   a concatenation of the <paramref name="scope" /> collection separated by
    ///   <see cref="SeparatorAsString" />.
    /// </returns>
    public static string ConvertScopeToString(IEnumerable<string> scope)
      => string.Join(SeparatorAsString, scope);

    /// <returns>
    ///   a string where all the leading backtrack strings are removed
    ///   from <paramref name="key" />.
    /// </returns>
    /// <remarks>
    ///   Given the key <c>../../a/b/c</c> and Scope <c>{ foo, bar, moo }</c>
    ///   the method will return <c>foo/a/b/c</c>.
    /// </remarks>
    public static string ToFullPath(string key, ICollection<string> scope)
      => IsAbsolute(key) ? key.Substring(1) : InterpretFromScope(key, scope);

    /// <remarks>
    ///   Given the key <c>../../a/b/c</c> and Scope <c>{ foo, bar, moo }</c>
    ///   the method will return <c>foo/a/b/c</c>.
    /// </remarks>
    public static string InterpretFromScope(string key, ICollection<string> scope) {
      if (IsAbsolute(key) || !scope.Any()) {
        return key;
      }
      var backtracks = CountBacktracks(key);
      if (backtracks >= scope.Count) {
        return key.Substring(scope.Count * BacktrackPath.Length);
      }
      var backtrackedScope = scope.Take(scope.Count - backtracks);
      var keyWithBacktracksRemoved = key.Substring(backtracks * BacktrackPath.Length);
      return ConvertScopeToString(backtrackedScope) + Separator + keyWithBacktracksRemoved;
    }

    public static T Get<T>(this IConf conf, Key<T> key) {
      var val = conf.TryGet(key);
      if (val.HasValue) {
        return val.Value;
      }
      throw new ConfUndefinedException($"Configuration '{key}' is undefined.");
    }
  }
}