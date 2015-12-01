namespace Bud.IO {
  public class InOutFile : IInOut {
    public string Path { get; }
    public bool IsOkay { get; }

    public InOutFile(string path, bool isOkay) {
      Path = path;
      IsOkay = isOkay;
    }

    public static InOutFile ToInOutFile(string file, bool isOkay) => new InOutFile(file, isOkay);

    public static InOutFile ToInOutFile(string file) => ToInOutFile(file, true);

    protected bool Equals(InOutFile other) => string.Equals(Path, other.Path) && IsOkay == other.IsOkay;

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) {
        return false;
      }
      if (ReferenceEquals(this, obj)) {
        return true;
      }
      return obj.GetType() == GetType() && Equals((InOutFile) obj);
    }

    public override int GetHashCode() {
      unchecked {
        return (Path.GetHashCode() * 397) ^ IsOkay.GetHashCode();
      }
    }

    public static bool operator ==(InOutFile left, InOutFile right) => Equals(left, right);

    public static bool operator !=(InOutFile left, InOutFile right) => !Equals(left, right);

    public override string ToString() => $"{GetType().Name}(Path: {Path}, IsOkay: {IsOkay})";
  }
}