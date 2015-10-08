using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bud.Configuration;

namespace Bud {
  public struct Configs : IEnumerable<IConfigTransform> {
    public static readonly Configs NewConfigs = new Configs(Enumerable.Empty<IConfigTransform>());
    private IEnumerable<IConfigTransform> ConfigTransforms { get; }

    public Configs(IEnumerable<IConfigTransform> configTransforms) {
      ConfigTransforms = configTransforms;
    }

    public IEnumerator<IConfigTransform> GetEnumerator() => ConfigTransforms.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) ConfigTransforms).GetEnumerator();
  }
}