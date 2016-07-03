using System.Collections.Generic;

namespace Bud.References {
  public interface IReferenceResolver {
    /// <param name="references">
    ///   the list of references to resolve. Each reference can be either an assembly name
    ///   or a path to an assembly file.
    /// </param>
    ResolvedReferences Resolve(IEnumerable<string> references);
  }
}