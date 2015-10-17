using System;
using Microsoft.CodeAnalysis;

namespace Bud.Compilation {
  public class AssemblyReference {
    public AssemblyReference(string path, MetadataReference metadataReference) {
      Path = path;
      MetadataReference = metadataReference;
    }

    public string Path { get; }
    public MetadataReference MetadataReference { get; }

    public override string ToString() => Path;

    public bool Equals(AssemblyReference other) => String.Equals(Path, other.Path);

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) {
        return false;
      }
      if (ReferenceEquals(this, obj)) {
        return true;
      }
      return obj.GetType() == typeof(AssemblyReference) && Equals((AssemblyReference) obj);
    }

    public override int GetHashCode() => Path.GetHashCode();

    public static bool operator ==(AssemblyReference left, AssemblyReference right) => Equals(left, right);

    public static bool operator !=(AssemblyReference left, AssemblyReference right) => !Equals(left, right);

    public static AssemblyReference CreateFromFile(string file)
      => new AssemblyReference(file, MetadataReference.CreateFromFile(file));
  }
}