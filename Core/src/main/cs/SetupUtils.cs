using System.Collections.Generic;
using System.Linq;

namespace Bud {
  public static class SetupUtils {
    public static Setup ToSetup(this IEnumerable<Setup> plugins) {
      return settings => plugins.Aggregate(settings, (oldSettings, plugin) => plugin(oldSettings));
    }
  }
}