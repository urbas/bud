namespace Bud.Configuration {
  public class NestedConfigs : IConfigs {
    private readonly string prefix;
    private readonly IConfigs configs;

    public NestedConfigs(string prefix, IConfigs configs) {
      this.prefix = prefix + "/";
      this.configs = configs;
    }

    public T Get<T>(Key<T> configKey) => configs.Get<T>(prefix + configKey);
  }
}