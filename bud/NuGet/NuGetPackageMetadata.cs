using System.Collections.Immutable;
using System.Linq;
using Bud.Collections;

namespace Bud.NuGet {
  public struct NuGetPackageMetadata {
    public string Authors { get; }
    public string Description { get; }
    public IImmutableDictionary<string, string> OptionalFields { get; }

    public NuGetPackageMetadata(string authors,
                                string description,
                                IImmutableDictionary<string, string> optionalFields) {
      Authors = authors;
      Description = description;
      OptionalFields = optionalFields ?? ImmutableDictionary<string, string>.Empty;
    }
  }
}