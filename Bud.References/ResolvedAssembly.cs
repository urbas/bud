namespace Bud.References {
  public class ResolvedAssembly {
    public string AssemblyName { get; }
    public string Path { get; }

    public ResolvedAssembly(string assemblyName, string path) {
      AssemblyName = assemblyName;
      Path = path;
    }

    public static ResolvedAssembly ToAssemblyPath(string path)
      => new ResolvedAssembly(System.IO.Path.GetFileNameWithoutExtension(path), path);

    protected bool Equals(ResolvedAssembly other) {
      return string.Equals(AssemblyName, other.AssemblyName) && string.Equals(Path, other.Path);
    }

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) {
        return false;
      }
      if (ReferenceEquals(this, obj)) {
        return true;
      }
      if (obj.GetType() != GetType()) {
        return false;
      }
      return Equals((ResolvedAssembly) obj);
    }

    public override int GetHashCode() {
      unchecked {
        return (AssemblyName.GetHashCode()*397) ^ Path.GetHashCode();
      }
    }
  }
}