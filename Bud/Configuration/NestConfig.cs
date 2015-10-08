namespace Bud.Configuration {
  public class NestConfig<T> : ConfigTransform<T> {
    public NestConfig(string prefix, ConfigTransform<T> configToNest) : base(prefix + "/" + configToNest.Key) {
      Prefix = prefix;
      ConfigToNest = configToNest;
    }

    private string Prefix { get; }
    private ConfigTransform<T> ConfigToNest { get; }

    public override ConfigDefinition<T> ToConfigDefinition() {
      var configDefinition = ConfigToNest.ToConfigDefinition();
      return new ConfigDefinition<T>(configs => configDefinition.Invoke(new NestedConfigs(Prefix, configs)));
    }

    public override ConfigDefinition<T> Modify(ConfigDefinition<T> configDefinition) {
      var modifiedConfigDefinition = ConfigToNest.Modify(configDefinition);
      return new ConfigDefinition<T>(configs => {
        var nestedConfigs = new NestedConfigs(Prefix, configs);
        return modifiedConfigDefinition.Invoke(nestedConfigs);
      });
    }
  }
}