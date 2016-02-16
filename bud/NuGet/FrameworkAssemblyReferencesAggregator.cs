using System;
using System.Collections.Generic;

namespace Bud.NuGet {
  public class FrameworkAssemblyReferencesAggregator {
    public static IDictionary<string, Version> AggregateReferences(
      IEnumerable<FrameworkAssemblyReference> frameworkAssemblyReferences) {
      var aggregatedReferences = new Dictionary<string, Version>();
      foreach (var assemblyToVersion in frameworkAssemblyReferences) {
        Version existingVersion;
        if (aggregatedReferences.TryGetValue(assemblyToVersion.AssemblyName, out existingVersion)) {
          if (assemblyToVersion.Framework > existingVersion) {
            aggregatedReferences[assemblyToVersion.AssemblyName] = assemblyToVersion.Framework;
          }
        } else {
          aggregatedReferences[assemblyToVersion.AssemblyName] = assemblyToVersion.Framework;
        }
      }
      return aggregatedReferences;
    }
  }
}