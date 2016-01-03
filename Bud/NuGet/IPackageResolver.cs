using System.Collections.Generic;

namespace Bud.NuGet {
  public interface IPackageResolver {
    IEnumerable<string> Resolve(IReadOnlyCollection<string> packagesConfigFiles, string cacheDir);
  }
}