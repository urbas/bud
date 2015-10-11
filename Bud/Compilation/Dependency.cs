using Microsoft.CodeAnalysis;

namespace Bud.Compilation {
  public class Dependency {
    public Dependency(string path, MetadataReference metadataReference) {
      Path = path;
      MetadataReference = metadataReference;
    }

    public string Path { get; }
    public MetadataReference MetadataReference { get; }

    public override string ToString() => Path;

    public bool Equals(Dependency other) => string.Equals(Path, other.Path);

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) {
        return false;
      }
      if (ReferenceEquals(this, obj)) {
        return true;
      }
      return obj.GetType() == typeof(Dependency) && Equals((Dependency) obj);
    }

    public override int GetHashCode() => Path.GetHashCode();

    public static bool operator ==(Dependency left, Dependency right) => Equals(left, right);

    public static bool operator !=(Dependency left, Dependency right) => !Equals(left, right);
  }
}