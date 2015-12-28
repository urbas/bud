using System.Collections.Immutable;

namespace Bud.NuGet {
  public class ResolvedAssemblies {
    public IImmutableSet<string> FrameworkReferences { get; }
    public IImmutableSet<string> Assemblies { get; }

    public ResolvedAssemblies(IImmutableSet<string> frameworkReferences, IImmutableSet<string> assemblies) {
      FrameworkReferences = frameworkReferences;
      Assemblies = assemblies;
    }
  }
}