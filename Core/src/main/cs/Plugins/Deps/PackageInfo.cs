using System.Runtime.Versioning;
using Newtonsoft.Json;
using NuGet;

namespace Bud.Plugins.Deps {
  public class PackageInfo {
    public readonly string Id;
    public readonly string Version;
    public readonly string Framework;
    [JsonIgnore] public readonly FrameworkName FrameworkName;
    [JsonIgnore] public readonly IVersionSpec VersionSpec;

    [JsonConstructor]
    public PackageInfo(string id, string version, string framework) {
      Id = id;
      Version = version;
      Framework = framework;
      FrameworkName = framework == null ? null : new FrameworkName(framework);
      VersionSpec = VersionUtility.ParseVersionSpec(version);
    }

    public PackageInfo(PackageDependency dependency, FrameworkName targetFramework) :
      this(dependency.Id, dependency.VersionSpec.ToString(), targetFramework == null ? null : targetFramework.FullName) { }
  }
}