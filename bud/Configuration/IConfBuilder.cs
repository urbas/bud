namespace Bud.Configuration {
  public interface IConfBuilder {
    void ApplyIn(ScopedDictionaryBuilder<IConfDefinition> configDefinitions);
  }
}