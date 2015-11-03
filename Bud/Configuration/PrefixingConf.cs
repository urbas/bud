namespace Bud.Configuration {
  internal class PrefixingConf : IConf {
    public ValueCache Cache { get; }
    public IConf Conf { get; }
    public string Prefix { get; }
    public int ScopeDepth { get; }

    public PrefixingConf(string prefix, int scopeDepth, IConf conf) {
      Prefix = prefix;
      ScopeDepth = scopeDepth;
      Conf = conf;
      Cache = new ValueCache(CalculateValue);
    }

    public T Get<T>(Key<T> key) => Cache.Get(key);

    private object CalculateValue(string key)
      => Conf.Get<object>(Keys.IsAbsolute(key) ? key : ResolveRelativePath(key));

    private string ResolveRelativePath(string key) {
      int backReferenceCount = 0;
      var keyPath = key;
      var maxBacktrackIndex = ScopeDepth * 3;
      while (backReferenceCount < maxBacktrackIndex && backReferenceCount + 3 <= keyPath.Length) {
        if (IsBackreferenceAtIndex(keyPath, backReferenceCount)) {
          backReferenceCount += 3;
        } else {
          break;
        }
      }
      return backReferenceCount > 0 ? keyPath.Substring(backReferenceCount) : Prefix + key;
    }

    private static bool IsBackreferenceAtIndex(string keyPath, int index)
      => keyPath[index] == '.' &&
         keyPath[index + 1] == '.' &&
         keyPath[index + 2] == Keys.Separator;
  }
}