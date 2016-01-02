using NuGet.Frameworks;

namespace Bud.NuGet {
  public struct FrameworkAssemblyReference {
    public string AssemblyName { get; }
    public NuGetFramework Framework { get; }

    public FrameworkAssemblyReference(string assemblyName, NuGetFramework framework) {
      AssemblyName = assemblyName;
      Framework = framework;
    }

    public override string ToString() => $"AssemblyName: {AssemblyName}, Framework: {Framework}";
  }
}