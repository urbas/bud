using System.Collections.Generic;
using System.Linq;

namespace Bud {
  public static class SettingsUtils {
    public static Setup ToPlugin(this IEnumerable<Setup> plugins) {
      return settings => plugins.Aggregate(settings, (oldSettings, plugin) => plugin(oldSettings));
    }
  }
}