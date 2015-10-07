namespace Bud.Configuration {
  public class NestConfig : IConfigTransform {
    private string Prefix { get; }
    private IConfigTransform ConfigToNest { get; }

    public NestConfig(string prefix, IConfigTransform configToNest) {
      Prefix = prefix;
      ConfigToNest = configToNest;
      Key = prefix + "/" + configToNest.Key;
    }

    public string Key { get; }

    public IConfigDefinition Modify(IConfigDefinition configDefinition) {
      var modifiedConfigDefinition = ConfigToNest.Modify(configDefinition);
      return new ConfigDefinition(modifiedConfigDefinition.ValueType, configs => {
        var nestedConfigs = new NestedConfigs(Prefix, configs);
        return modifiedConfigDefinition.Invoke(nestedConfigs);
      });
    }

    public IConfigDefinition ToConfigDefinition() {
      var configDefinition = ConfigToNest.ToConfigDefinition();
      return new ConfigDefinition(configDefinition.ValueType, configs => configDefinition.Invoke(new NestedConfigs(Prefix, configs)));
    }
  }
}