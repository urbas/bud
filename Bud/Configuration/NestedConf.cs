namespace Bud.Configuration {
  public class NestedConf : IConf {
    private readonly string prefix;
    private readonly IConf conf;

    public NestedConf(string prefix, IConf conf) {
      this.prefix = prefix + "/";
      this.conf = conf;
    }

    public T Get<T>(Key<T> configKey) {
      if (configKey.IsAbsolute) {
        return conf.Get(configKey);
      }
      return conf.Get<T>(prefix + configKey);
    }
  }
}