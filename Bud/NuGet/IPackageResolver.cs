using System.Collections.Generic;

namespace Bud.NuGet {
  public interface IPackageResolver {
    IEnumerable<string> Resolve(ICollection<string> packagesConfigFiles, string cacheDir);
  }
}