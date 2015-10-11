using System.Collections.Generic;
using System.Reactive;

namespace Bud.Compilation {
  public class CompilationInput {
    public IEnumerable<string> Sources { get; }
    public IEnumerable<Timestamped<Dependency>> Dependencies { get; }

    public CompilationInput(IEnumerable<string> sources, IEnumerable<Timestamped<Dependency>> dependencies) {
      Sources = sources;
      Dependencies = dependencies;
    }

    public static CompilationInput Create(IEnumerable<string> sources, IEnumerable<Timestamped<Dependency>> references)
      => new CompilationInput(sources, references);
  }
}