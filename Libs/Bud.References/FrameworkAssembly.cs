using System;
using System.Collections.Generic;

namespace Bud.References {
  public class FrameworkAssembly {
    public static readonly Version MaxVersion = new Version(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);
    public string Name { get; }
    public Version FrameworkVersion { get; }

    public FrameworkAssembly(string name, Version frameworkVersion) {
      Name = name;
      FrameworkVersion = frameworkVersion;
    }

    public override string ToString()
      => $"{GetType().Name}(AssemblyName: {Name}, Framework: {FrameworkVersion})";

    protected bool Equals(FrameworkAssembly other)
      => string.Equals(Name, other.Name) &&
         FrameworkVersion.Equals(other.FrameworkVersion);

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) {
        return false;
      }
      if (ReferenceEquals(this, obj)) {
        return true;
      }
      if (obj.GetType() != GetType()) {
        return false;
      }
      return Equals((FrameworkAssembly) obj);
    }

    public override int GetHashCode() {
      unchecked {
        return (Name.GetHashCode()*397) ^ FrameworkVersion.GetHashCode();
      }
    }

    /// <summary>
    ///   Takes entries from the given list of framework assemblies, discards duplicate framework assemblies
    ///   (by choosing the one with the highest version), and puts them into the resulting list.
    /// </summary>
    /// <param name="assemblies">
    ///   a list of assemblies. Each assembly is associated with a framework version.
    ///   This list may contain duplicate assemblies with different framework versions.
    /// </param>
    /// <returns>
    ///   a list that contains all assemblies specified in <paramref name="assemblies" /> but
    ///   with removed duplicates. If any duplicates are found in <paramref name="assemblies" />
    ///   then the one with the highest version is chosen.
    /// </returns>
    public static IEnumerable<FrameworkAssembly> TakeHighestVersions(IEnumerable<FrameworkAssembly> assemblies) {
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