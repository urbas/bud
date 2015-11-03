namespace Bud.Configuration {
  internal class PrefixedConfDefinition : IConfDefinition {
    public string Prefix { get; }
    public int ScopeDepth { get; }
    public IConfDefinition ConfDefinition { get; }

    public PrefixedConfDefinition(string prefix, int scopeDepth, IConfDefinition confDefinition) {
      Prefix = prefix;
      ScopeDepth = scopeDepth;
      ConfDefinition = confDefinition;
    }

    public object Value(IConf conf)
      => ConfDefinition.Value(new PrefixingConf(Prefix, ScopeDepth, conf));
  }
}