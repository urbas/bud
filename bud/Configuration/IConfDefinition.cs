using Bud.V1;

namespace Bud.Configuration {
  public interface IConfDefinition {
    object Value(IConf conf);
  }
}