using System.Collections.Generic;

namespace Bud.Configuration {
  public interface IConfigTransform {
    void ApplyIn(IDictionary<string, IConfigDefinition> configDefinitions);
  }
}