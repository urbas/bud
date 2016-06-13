using System.Collections.Generic;

namespace Bud.References {
  public class AssemblyAggregator {
    /// <param name="assemblies">
    ///   a list of assemblies. Each assembly is associated with a framework version.
    ///   This list may contain duplicate assemblies with different framework versions.
    /// </param>
    /// <returns>
    ///   a list that contains all assemblies specified in <paramref name="assemblies" /> but
    ///   with removed duplicates. If any duplicates are found in <paramref name="assemblies" />
    ///   then the one with the highest version is chosen.
    /// </returns>
    public static IEnumerable<FrameworkAssembly> AggregateByFrameworkVersion(
      IEnumerable<FrameworkAssembly> assemblies) {
      var aggregatedAssemblies = new Dictionary<string, FrameworkAssembly>();
      foreach (var assemblyToVersion in assemblies) {
        var existingAssembly = aggregatedAssemblies.Get(assemblyToVersion.Name);
        if (!existingAssembly.HasValue ||
            assemblyToVersion.FrameworkVersion > existingAssembly.Value.FrameworkVersion) {
          aggregatedAssemblies[assemblyToVersion.Name] = assemblyToVersion;
        }
      }
      return aggregatedAssemblies.Values;
    }
  }
}