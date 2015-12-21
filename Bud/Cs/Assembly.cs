namespace Bud.Cs {
  public class Assembly {
    public string Path { get; }

    public Assembly(string path) {
      Path = path;
    }

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) {
        return false;
      }
      if (ReferenceEquals(this, obj)) {
        return true;
      }
      return obj.GetType() == GetType() && Equals((Assembly) obj);
    }

    protected bool Equals(Assembly other) => string.Equals(Path, other.Path);
    public override int GetHashCode() => Path.GetHashCode();
    public static Assembly ToAssembly(string dllPath) => new Assembly(dllPath);
  }
}