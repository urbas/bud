using System.Collections.Generic;

namespace Bud {
  public static class SetupUtils {
    public static Setup Merge(this IEnumerable<Setup> plugins) {
      return settings => settings.Add(plugins);
    }

    public static Setup Merge(params Setup[] setups) {
      return settings => settings.Add(setups);
    }
  }
}