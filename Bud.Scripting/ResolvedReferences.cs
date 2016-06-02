using System.Collections.Generic;

namespace Bud.Scripting {
  public class ResolvedReferences {
    public ResolvedReferences(IReadOnlyDictionary<string, string> assemblyReferences,
                       IReadOnlyDictionary<string, string> frameworkAssemblyReferences) {
      AssemblyReferences = assemblyReferences;
      FrameworkAssemblyReferences = frameworkAssemblyReferences;
    }

    public IReadOnlyDictionary<string, string> AssemblyReferences { get; }
    public IReadOnlyDictionary<string, string> FrameworkAssemblyReferences { get; }
  }
}