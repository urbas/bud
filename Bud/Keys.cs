using System;

namespace Bud {
  public static class Keys {
    public static readonly Key Root = new Key("");
    public static char Separator => '/';

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
  }
}