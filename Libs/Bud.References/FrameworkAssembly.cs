using System;

namespace Bud.References {
  public class FrameworkAssembly {
    public static readonly Version MaxVersion = new Version(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);
    public string Name { get; }
    public Version FrameworkVersion { get; }

    public FrameworkAssembly(string name, Version frameworkVersion) {
      Name = name;
      FrameworkVersion = frameworkVersion;
    }

    public override string ToString()
      => $"{GetType().Name}(AssemblyName: {Name}, Framework: {FrameworkVersion})";

    protected bool Equals(FrameworkAssembly other)
      => string.Equals(Name, other.Name) &&
         FrameworkVersion.Equals(other.FrameworkVersion);

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
      return Equals((FrameworkAssembly) obj);
    }

    public override int GetHashCode() {
      unchecked {
        return (Name.GetHashCode()*397) ^ FrameworkVersion.GetHashCode();
      }
    }
  }
}