using System.Collections.Generic;
using System.Linq;
using Bud.IO;
using Newtonsoft.Json;
using NuGet;

namespace Bud.Dependencies {
  public class JsonAssembly {
    public readonly string Name;
    public readonly string Path;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public readonly string Framework;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public readonly IEnumerable<string> SupportedFrameworks;

    [JsonConstructor]
    private JsonAssembly(string name, string path, string framework, IEnumerable<string> supportedFrameworks) {
      Name = name;
      Path = path;
      Framework = framework;
      SupportedFrameworks = supportedFrameworks == null || supportedFrameworks.IsEmpty() ? null : supportedFrameworks;
    }

    public JsonAssembly(IPackageAssemblyReference assemblyReference)
      : this(assemblyReference.Name,
             Paths.ToAgnosticPath(assemblyReference.Path),
             assemblyReference.TargetFramework?.FullName,
             assemblyReference.SupportedFrameworks.Select(framework => framework.FullName)) {}
  }
}