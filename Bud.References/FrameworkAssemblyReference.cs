using System;

namespace Bud.References {
  public class FrameworkAssemblyReference {
    public static readonly Version MaxVersion = new Version(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);
    public string AssemblyName { get; }
    public Version FrameworkVersion { get; }

    public FrameworkAssemblyReference(string assemblyName, Version frameworkVersion) {
      AssemblyName = assemblyName;
      FrameworkVersion = frameworkVersion;
    }

    public override string ToString()
      => $"{GetType().Name}(AssemblyName: {AssemblyName}, Framework: {FrameworkVersion})";

    protected bool Equals(FrameworkAssemblyReference other)
      => string.Equals(AssemblyName, other.AssemblyName) &&
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
      return Equals((FrameworkAssemblyReference) obj);
    }

    public override int GetHashCode() {
      unchecked {
        return (AssemblyName.GetHashCode()*397) ^ FrameworkVersion.GetHashCode();
      }
    }
  }
}