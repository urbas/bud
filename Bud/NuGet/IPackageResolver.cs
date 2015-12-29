using System.Collections.Generic;

namespace Bud.NuGet {
  public interface IPackageResolver {
    IEnumerable<string> Resolve(IEnumerable<string> packagesConfigFiles,
                                string cacheDir);
  }
}