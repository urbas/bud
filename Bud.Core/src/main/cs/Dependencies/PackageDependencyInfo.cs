using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Versioning;
using Newtonsoft.Json;
using NuGet;

namespace Bud.Dependencies {
  public class PackageDependencyInfo {
    public readonly string Id;
    public readonly string Version;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public readonly string Framework;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public readonly IEnumerable<string> SupportedFrameworks;
    [JsonIgnore] public readonly FrameworkName FrameworkName;
    [JsonIgnore] public readonly IEnumerable<FrameworkName> SupportedFrameworkNames;
    [JsonIgnore] public readonly IVersionSpec VersionSpec;

    [JsonConstructor]
    public PackageDependencyInfo(string id, string version, string framework, IEnumerable<string> supportedFrameworks) {
      Id = id;
      Version = version;
      Framework = framework;
      SupportedFrameworks = supportedFrameworks == null || supportedFrameworks.IsEmpty() ? null : supportedFrameworks;
      FrameworkName = framework == null ? null : new FrameworkName(framework);
      SupportedFrameworkNames = supportedFrameworks?.Select(supportedFramework => new FrameworkName(supportedFramework)) ?? ImmutableList<FrameworkName>.Empty;
      VersionSpec = VersionUtility.ParseVersionSpec(version);
    }

    public PackageDependencyInfo(PackageDependency dependency, FrameworkName targetFramework, IEnumerable<FrameworkName> supportedFrameworks) :
      this(dependency.Id, dependency.VersionSpec.ToString(), targetFramework?.FullName, supportedFrameworks.Select(framework => framework.FullName)) {}
  }
}