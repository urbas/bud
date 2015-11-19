namespace Bud.Configuration {
  public abstract class ConfBuilder : IConfBuilder {
    protected ConfBuilder(string key) {
      Key = key;
    }

    public string Key { get; }
    public abstract void ApplyIn(ScopedDictionaryBuilder<IConfDefinition> configDefinitions);
  }
}