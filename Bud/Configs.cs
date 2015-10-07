using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bud.Configuration;

namespace Bud {
  public struct Configs : IEnumerable<IConfigTransform> {
    public static readonly Configs NewConfigs = new Configs(Enumerable.Empty<IConfigTransform>());
    private IEnumerable<IConfigTransform> ConfigModifications { get; }

    public Configs(IEnumerable<IConfigTransform> configModifications) {
      ConfigModifications = configModifications;
    }

    public IEnumerator<IConfigTransform> GetEnumerator() => ConfigModifications.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) ConfigModifications).GetEnumerator();
  }
}