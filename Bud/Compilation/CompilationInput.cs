using System.Collections.Generic;
using System.Linq;
using Bud.IO;

namespace Bud.Compilation {
  public struct CompilationInput {
    public CompilationInput(Files sources, Assemblies assemblies) {
      Sources = sources.Select(Files.ToTimeHashedFile).ToList();
      Assemblies = assemblies.Select(reference => reference.ToHashed()).ToList();
    }

    public List<Hashed<string>> Sources { get; }
    public List<Hashed<AssemblyReference>> Assemblies { get; }
  }
}