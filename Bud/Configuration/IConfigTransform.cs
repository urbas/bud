using System.Collections.Generic;

namespace Bud.Configuration {
  public interface IConfBuilder {
    void ApplyIn(IDictionary<string, IConfigDefinition> configDefinitions);
  }
}