namespace Bud.NuGet {
  public class PackageDependency {
    public string PackageId { get; }
    public string Version { get; }

    public PackageDependency(string packageId, string version) {
      PackageId = packageId;
      Version = version;
    }

    public bool Equals(PackageDependency other)
      => string.Equals(PackageId, other.PackageId)
      && string.Equals(Version, other.Version);

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) {
        return false;
      }
      if (ReferenceEquals(this, obj)) {
        return true;
      }
      return obj.GetType() == GetType() && Equals((PackageDependency) obj);
    }

    public static bool operator ==(PackageDependency left, PackageDependency right)
      => Equals(left, right);

    public static bool operator !=(PackageDependency left, PackageDependency right)
      => !Equals(left, right);

    public override int GetHashCode() {
      unchecked {
        return (PackageId.GetHashCode()*397) ^ Version.GetHashCode();
      }
    }

    public override string ToString()
      => $"PackageId: {PackageId}, Version: {Version}";
  }
}