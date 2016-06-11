using System.Collections.Generic;

namespace Bud.References {
  public interface IReferenceResolver {
    /// <param name="references">
    ///   the list of references to resolve.
    /// </param>
    /// <returns>
    ///   a dictionary where the keys are reference names and the values are optional paths.
    ///   If the reference has not been resolved, its path will be <see cref="Option.None{T}"/>.
    ///   If the reference was successfully resolved, then its path will be a string.
    /// </returns>
    IDictionary<string, Option<string>> Resolve(IEnumerable<string> references);
  }
}