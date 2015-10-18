using System.Linq;
using Bud.IO;
using Microsoft.CodeAnalysis;

namespace Bud.Compilation {
  public static class DependencyObservatory {
    public static Assemblies ObserveAssemblies(params string[] locations)
      => new Assemblies(locations.Select(ToTimestampedDependency));

    private static Hashed<AssemblyReference> ToTimestampedDependency(string file)
      => new Hashed<AssemblyReference>(new AssemblyReference(file, MetadataReference.CreateFromFile(file)), Files.GetTimeHash(file));
  }
}