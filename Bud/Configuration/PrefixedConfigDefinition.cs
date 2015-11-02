namespace Bud.Configuration {
  internal class PrefixedConfigDefinition : IConfigDefinition {
    public string Prefix { get; }
    public int ScopeDepth { get; }
    public IConfigDefinition ConfigDefinition { get; }

    public PrefixedConfigDefinition(string prefix, int scopeDepth, IConfigDefinition configDefinition) {
      Prefix = prefix;
      ScopeDepth = scopeDepth;
      ConfigDefinition = configDefinition;
    }

    public object Value(IConf conf)
      => ConfigDefinition.Value(new PrefixingConf(Prefix, ScopeDepth, conf));
  }
}