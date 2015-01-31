using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using Bud.IO;
using Newtonsoft.Json;
using NuGet;

namespace Bud.Dependencies {
  public class AssemblyRereference : IPackageAssemblyReference {
    [JsonProperty(PropertyName = "Name")] private readonly string name;
    [JsonProperty(PropertyName = "Path")] private readonly string relativePath;
    [JsonProperty(PropertyName = "Framework", NullValueHandling = NullValueHandling.Ignore)] private readonly string targetFramework;
    [JsonProperty(PropertyName = "SupportedFrameworks", NullValueHandling = NullValueHandling.Ignore)] private readonly IEnumerable<string> supportedFrameworks;
    [JsonIgnore] internal Package HostPackage;
    private string absolutePath;

    [JsonConstructor]
    private AssemblyRereference(string name, string path, string framework, IEnumerable<string> supportedFrameworks) {
      this.name = name;
      relativePath = path;
      targetFramework = framework;
      this.supportedFrameworks = supportedFrameworks == null || supportedFrameworks.IsEmpty() ? null : supportedFrameworks;
      TargetFramework = framework == null ? null : new FrameworkName(framework);
      SupportedFrameworks = supportedFrameworks == null ? ImmutableList<FrameworkName>.Empty : supportedFrameworks.Select(supportedFramework => new FrameworkName(supportedFramework));
    }

    public AssemblyRereference(IPackageAssemblyReference assemblyReference)
      : this(assemblyReference.Name,
             Paths.ToAgnosticPath(assemblyReference.Path),
             assemblyReference.TargetFramework == null ? null : assemblyReference.TargetFramework.FullName,
             assemblyReference.SupportedFrameworks.Select(framework => framework.FullName)) {}

    internal AssemblyRereference WithHostPackage(Package package) {
      HostPackage = package;
      return this;
    }

    [JsonIgnore]
    public IEnumerable<FrameworkName> SupportedFrameworks { get; private set; }

    public Stream GetStream() {
      throw new NotImplementedException();
    }

    [JsonIgnore]
    public string Path {
      get {
        if (absolutePath == null) {
          absolutePath = System.IO.Path.Combine(Config.GetFetchedDependenciesDir(), HostPackage.Id + "." + HostPackage.Version, relativePath);
        }
        return absolutePath;
      }
    }

    private IConfig Config {
      get { return HostPackage.HostPackageVersions.HostFetchedDependencies.Config; }
    }

    [JsonIgnore]
    public string EffectivePath {
      get { return Path; }
    }

    [JsonIgnore]
    public FrameworkName TargetFramework { get; private set; }

    [JsonIgnore]
    public string Name {
      get { return name; }
    }
  }
}