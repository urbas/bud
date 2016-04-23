namespace Bud.Configuration {
  public interface IConfBuilder {
    void ApplyIn(DirectoryDictionary<IConfDefinition> configDefinitions);
  }
}