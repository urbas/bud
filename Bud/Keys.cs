namespace Bud {
  public static class Keys {
    public static readonly Key Root = new Key("");

    public static Key<T> ToAbsolute<T>(this Key<T> configKey) {
      if (configKey.IsAbsolute) {
        return configKey;
      }
      return "/" + configKey.Id;
    }
  }
}