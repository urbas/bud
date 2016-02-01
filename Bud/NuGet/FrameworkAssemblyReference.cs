using System;

namespace Bud.NuGet {
  public struct FrameworkAssemblyReference {
    public string AssemblyName { get; }
    public Version Framework { get; }

    public FrameworkAssemblyReference(string assemblyName, Version framework) {
      AssemblyName = assemblyName;
      Framework = framework;
    }

    public override string ToString()
      => $"FrameworkAssemblyReference(AssemblyName: {AssemblyName}, Framework: {Framework})";
  }
}