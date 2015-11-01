namespace Bud.Configuration {
  internal class PrefixedConfigDefinition : IConfigDefinition {
    public string Prefix { get; }
    public IConfigDefinition ConfigDefinition { get; }

    public PrefixedConfigDefinition(string prefix,
                                    IConfigDefinition configDefinition) {
      Prefix = prefix;
      ConfigDefinition = configDefinition;
    }

    public object Value(IConf conf)
      => ConfigDefinition.Value(new PrefixingConf(Prefix, conf));
  }
}