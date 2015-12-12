namespace Bud.Cs {
  public class Package {
    public string PackageId { get; }
    public string Version { get; }
    public string TargetFramework { get; }

    public Package(string packageId, string version, string targetFramework) {
      PackageId = packageId;
      Version = version;
      TargetFramework = targetFramework;
    }

    protected bool Equals(Package other)
      => string.Equals(PackageId, other.PackageId) &&
         string.Equals(Version, other.Version) &&
         string.Equals(TargetFramework, other.TargetFramework);

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) {
        return false;
      }
      if (ReferenceEquals(this, obj)) {
        return true;
      }
      return obj.GetType() == GetType() && Equals((Package) obj);
    }

    public override int GetHashCode() {
      unchecked {
        var hashCode = PackageId.GetHashCode();
        hashCode = (hashCode * 397) ^ Version.GetHashCode();
        hashCode = (hashCode * 397) ^ TargetFramework.GetHashCode();
        return hashCode;
      }
    }

    public static bool operator ==(Package left, Package right) => Equals(left, right);
    public static bool operator !=(Package left, Package right) => !Equals(left, right);
    public override string ToString() => $"Package({PackageId}, {Version}, {TargetFramework})";
  }
}