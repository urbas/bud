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
  }
}