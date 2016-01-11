namespace Bud.NuGet {
  public class PackageDependency {
    public string PackageId { get; }
    public string Version1 { get; }

    public PackageDependency(string packageId, string version) {
      PackageId = packageId;
      Version1 = version;
    }

    public bool Equals(PackageDependency other)
      => string.Equals(PackageId, other.PackageId)
      && string.Equals(Version1, other.Version1);

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) {
        return false;
      }
      if (ReferenceEquals(this, obj)) {
        return true;
      }
      return obj.GetType() == GetType() && Equals((PackageDependency) obj);
    }

    public override int GetHashCode() {
      unchecked {
        return (PackageId.GetHashCode()*397) ^ Version1.GetHashCode();
      }
    }
  }
}