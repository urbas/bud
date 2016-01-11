namespace Bud.NuGet {
  public class PackageFile {
    public string FileToPackage { get; }
    public string PathInPackage { get; }

    public PackageFile(string fileToPackage, string pathInPackage) {
      FileToPackage = fileToPackage;
      PathInPackage = pathInPackage;
    }

    public bool Equals(PackageFile other) {
      return string.Equals(FileToPackage, other.FileToPackage) && string.Equals(PathInPackage, other.PathInPackage);
    }

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) {
        return false;
      }
      if (ReferenceEquals(this, obj)) {
        return true;
      }
      if (obj.GetType() != this.GetType()) {
        return false;
      }
      return Equals((PackageFile) obj);
    }

    public override int GetHashCode() {
      unchecked {
        return (FileToPackage.GetHashCode()*397) ^ PathInPackage.GetHashCode();
      }
    }

    public static bool operator ==(PackageFile left, PackageFile right) {
      return Equals(left, right);
    }

    public static bool operator !=(PackageFile left, PackageFile right) {
      return !Equals(left, right);
    }

    public override string ToString() {
      return $"FileToPackage: {FileToPackage}, PathInPackage: {PathInPackage}";
    }
  }
}