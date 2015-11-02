namespace Bud.Configuration {
  internal class PrefixingConf : IConf {
    public IConf Conf { get; }
    public string Prefix { get; }
    public int ScopeDepth { get; }

    public PrefixingConf(string prefix, int scopeDepth, IConf conf) {
      Prefix = prefix;
      ScopeDepth = scopeDepth;
      Conf = conf;
    }

    public T Get<T>(Key<T> key)
      => Conf.Get(key.IsAbsolute ? key : ResolveRelativePath(key));

    private Key<T> ResolveRelativePath<T>(Key<T> key) {
      int backReferenceCount = 0;
      var keyPath = key.Id;
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