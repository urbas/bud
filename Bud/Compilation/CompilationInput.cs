using System.Collections.Generic;
using System.Reactive;

namespace Bud.Compilation {
  public class CompilationInput {
    public CompilationInput(IEnumerable<string> sources, IEnumerable<Timestamped<Dependency>> dependencies) {
      Sources = sources;
      Dependencies = dependencies;
    }

    public IEnumerable<string> Sources { get; }
    public IEnumerable<Timestamped<Dependency>> Dependencies { get; }

    public static CompilationInput Create(IEnumerable<string> enumerable, IEnumerable<Timestamped<Dependency>> references) {
      return new CompilationInput(enumerable, references);
    }
  }
}