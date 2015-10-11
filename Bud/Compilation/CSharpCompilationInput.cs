using System.Collections.Generic;
using System.Reactive;

namespace Bud.Compilation {
  public class CSharpCompilationInput {
    public CSharpCompilationInput(IEnumerable<string> sources, IEnumerable<Timestamped<Dependency>> dependencies) {
      Sources = sources;
      Dependencies = dependencies;
    }

    public IEnumerable<string> Sources { get; }
    public IEnumerable<Timestamped<Dependency>> Dependencies { get; }
  }
}