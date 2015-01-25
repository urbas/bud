using System.Runtime.Versioning;
using Bud.IO;
using Newtonsoft.Json;
using NuGet;

namespace Bud.Dependencies {
  public class AssemblyRereference {
    public readonly string Path;
    public readonly string Framework;
    [JsonIgnore] public readonly FrameworkName FrameworkName;
    [JsonIgnore] internal Package HostPackage;

    [JsonConstructor]
    private AssemblyRereference(string path, string framework) {
      Path = path;
      Framework = framework;
      FrameworkName = framework == null ? null : new FrameworkName(framework);
    }

    public AssemblyRereference(IPackageAssemblyReference assemblyReference)
      : this(Paths.ToAgnosticPath(assemblyReference.Path), assemblyReference.TargetFramework == null ? null : assemblyReference.TargetFramework.FullName) {}

    public string GetAbsolutePath(string nuGetRepositoryPath) {
      return System.IO.Path.Combine(nuGetRepositoryPath, HostPackage.Id + "." + HostPackage.Version, Path);
    }

    internal AssemblyRereference WithHostPackage(Package package) {
      HostPackage = package;
      return this;
    }
  }
}