using System.Collections.Generic;
using NuGet.Packaging;

namespace Bud.V1 {
  public class PackageReferenceComparer : IEqualityComparer<PackageReference> {
    public bool Equals(PackageReference x, PackageReference y)
      => x.PackageIdentity.Equals(y.PackageIdentity) &&
         x.TargetFramework.Equals(y.TargetFramework);

    public int GetHashCode(PackageReference obj)
      => obj.PackageIdentity.GetHashCode()*37 +
         obj.TargetFramework.GetHashCode();
  }
}