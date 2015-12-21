namespace Bud.IO {
  public class InOutFile : IInOut {
    public string Path { get; }

    public InOutFile(string path) {
      Path = path;
    }

    public static InOutFile ToInOutFile(string file) => new InOutFile(file);

    protected bool Equals(InOutFile other) => string.Equals(Path, other.Path);

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) {
        return false;
      }
      if (ReferenceEquals(this, obj)) {
        return true;
      }
      return obj.GetType() == GetType() && Equals((InOutFile) obj);
    }

    public override int GetHashCode() => Path.GetHashCode();

    public static bool operator ==(InOutFile left, InOutFile right) => Equals(left, right);

    public static bool operator !=(InOutFile left, InOutFile right) => !Equals(left, right);

    public override string ToString() => $"{GetType().Name}(Path: {Path})";
  }
}