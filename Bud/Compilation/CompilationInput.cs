using Bud.IO;

namespace Bud.Compilation {
  public struct CompilationInput {
    public CompilationInput(Files sources, Assemblies assemblies) {
      Sources = sources;
      Assemblies = assemblies;
    }

    public Files Sources { get; }
    public Assemblies Assemblies { get; }

    public static CompilationInput Create(Files sources, Assemblies dependencies) {
      return new CompilationInput(sources, dependencies);
    }
  }
}