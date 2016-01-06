using System.Collections.Generic;

namespace Bud.NuGet {
  public interface INuGetPublisher {
    void Publish(string packageId, string version, IEnumerable<string> files);
  }
}