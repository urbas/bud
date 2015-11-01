namespace Bud.Configuration {
  internal class PrefixingConf : IConf {
    public IConf Conf { get; }
    public string Prefix { get; }

    public PrefixingConf(string prefix, IConf conf) {
      Prefix = prefix;
      Conf = conf;
    }

    public T Get<T>(Key<T> configKey)
      => Conf.Get<T>(configKey.IsAbsolute ? configKey.Id : Prefix + configKey);
  }
}