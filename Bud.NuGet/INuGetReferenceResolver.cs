using System.Collections.Immutable;
using Bud.References;

namespace Bud.NuGet {
  public interface INuGetReferenceResolver {
    /// <summary>
    ///   Places resolved dlls in the output directory.
    /// </summary>
    /// <param name="packageReferences">
    ///   these references will be downloaded.
    /// </param>
    /// <param name="outputDir">
    ///   the directory where this resolver will place its caches and
    ///   downloaded files.
    /// </param>
    /// <returns>
    ///   a dictionary of reference ID's and assembly paths.
    /// </returns>
    ResolvedReferences Resolve(ImmutableArray<PackageReference> packageReferences,
                               string outputDir);
  }
}