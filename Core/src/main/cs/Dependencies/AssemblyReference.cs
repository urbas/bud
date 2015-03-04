using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.IO.Path;
using System.Linq;
using System.Runtime.Versioning;
using Bud.IO;
using Newtonsoft.Json;
using NuGet;

namespace Bud.Dependencies {
  public class AssemblyReference : IPackageAssemblyReference {
    [JsonProperty(PropertyName = "Name")] private readonly string name;
    [JsonProperty(PropertyName = "Path")] private readonly string relativePath;
    [JsonProperty(PropertyName = "Framework", NullValueHandling = NullValueHandling.Ignore)] private readonly string targetFramework;
    [JsonProperty(PropertyName = "SupportedFrameworks", NullValueHandling = NullValueHandling.Ignore)] private readonly IEnumerable<string> supportedFrameworks;

    [JsonIgnore]
    public Package HostPackage { get; private set; }

    private string AbsolutePath;

    [JsonConstructor]
    private AssemblyReference(string name, string path, string framework, IEnumerable<string> supportedFrameworks) {
      this.name = name;
      relativePath = path;
      targetFramework = framework;
      this.supportedFrameworks = supportedFrameworks == null || supportedFrameworks.IsEmpty() ? null : supportedFrameworks;
      TargetFramework = framework == null ? null : new FrameworkName(framework);
      SupportedFrameworks = supportedFrameworks == null ? ImmutableList<FrameworkName>.Empty : supportedFrameworks.Select(supportedFramework => new FrameworkName(supportedFramework));
    }

    public AssemblyReference(IPackageAssemblyReference assemblyReference)
      : this(assemblyReference.Name,
             Paths.ToAgnosticPath(assemblyReference.Path),
             assemblyReference.TargetFramework == null ? null : assemblyReference.TargetFramework.FullName,
             assemblyReference.SupportedFrameworks.Select(framework => framework.FullName)) {}

    internal AssemblyReference WithHostPackage(Package package) {
      HostPackage = package;
      return this;
    }

    [JsonIgnore]
    public IEnumerable<FrameworkName> SupportedFrameworks { get; }

    public Stream GetStream() {
      throw new NotImplementedException();
    }

    [JsonIgnore]
    public string Path {
      get {
        if (AbsolutePath == null) {
          AbsolutePath = Combine(Config.GetFetchedDependenciesDir(), HostPackage.Id + "." + HostPackage.Version, relativePath);
        }
        return AbsolutePath;
      }
    }

    private IConfig Config => HostPackage.HostPackageVersions.HostFetchedDependencies.Config;

    [JsonIgnore]
    public string EffectivePath => Path;

    [JsonIgnore]
    public FrameworkName TargetFramework { get; }

    [JsonIgnore]
    public string Name => name;
  }
}