namespace Bud.Configuration {
  public interface IConfigTransform {
    string Key { get; }
    IConfigDefinition ToConfigDefinition();
    IConfigDefinition Modify(IConfigDefinition configDefinition);
    IConfigTransform Nest(string parentKey);
  }
}