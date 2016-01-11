using System.Collections.Generic;

namespace Bud.NuGet {
  public interface IPublisher {
    void Publish(string packageId,
                 string version,
                 IEnumerable<PackageFile> files,
                 IEnumerable<PackageDependency> packageDependencies);
  }
}