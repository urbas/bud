namespace Bud.Configuration {
  public interface IConfBuilder {
    void AddTo(DirectoryDictionary<IConfDefinition> configDefinitions);
  }
}