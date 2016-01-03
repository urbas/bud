using System;
using System.Collections.Generic;

namespace Bud.NuGet {
  public class FrameworkAssemblyReferencesAggregator {
    public static IDictionary<string, Version> AggregateReferences(
      IEnumerable<Tuple<string, Version>> frameworkAssemblyReferences) {
      var aggregatedReferences = new Dictionary<string, Version>();
      foreach (var assemblyToVersion in frameworkAssemblyReferences) {
        Version existingVersion;
        if (aggregatedReferences.TryGetValue(assemblyToVersion.Item1, out existingVersion)) {
          if (assemblyToVersion.Item2 > existingVersion) {
            aggregatedReferences[assemblyToVersion.Item1] = assemblyToVersion.Item2;
          }
        } else {
          aggregatedReferences[assemblyToVersion.Item1] = assemblyToVersion.Item2;
        }
      }
      return aggregatedReferences;
    }
  }
}