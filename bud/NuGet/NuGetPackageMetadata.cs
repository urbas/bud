using System.Collections.Immutable;

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

    /// <summary>
    ///   Adds or overwrites the optional metadata field. See https://docs.nuget.org/create/nuspec-reference#metadata-section
    ///   for more information on walid metadata fields.
    /// </summary>
    public NuGetPackageMetadata WithField(string key, string value)
      => new NuGetPackageMetadata(
      Authors,
      Description,
      OptionalFields.SetItem(key, value));
  }
}