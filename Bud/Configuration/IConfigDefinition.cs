namespace Bud.Configuration {
  public interface IConfigDefinition {
    object Value(IConf conf);
  }
}