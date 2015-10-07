namespace Bud.Configuration {
  public interface IConfigTransform {
    string Key { get; }
    IConfigDefinition Modify(IConfigDefinition configDefinition);
    IConfigDefinition ToConfigDefinition();
  }
}