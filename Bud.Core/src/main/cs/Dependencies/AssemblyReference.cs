using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.IO.Path;
using System.Linq;
using System.Runtime.Versioning;
using NuGet;

namespace Bud.Dependencies {
  public class AssemblyReference : IPackageAssemblyReference {
    public readonly string AbsolutePath;

    public AssemblyReference(string fetchedDependenciesDir, string packageId, string version, JsonAssembly jsonAssembly) {
      AbsolutePath = FetchedDependenciesUtil.FetchedAssemblyAbsolutePath(fetchedDependenciesDir, packageId, version, jsonAssembly.Path);
      TargetFramework = jsonAssembly.Framework == null ? null : new FrameworkName(jsonAssembly.Framework);
      SupportedFrameworks = jsonAssembly.SupportedFrameworks?.Select(supportedFramework => new FrameworkName(supportedFramework)) ?? ImmutableList<FrameworkName>.Empty;
      Name = jsonAssembly.Name;
    }

    public IEnumerable<FrameworkName> SupportedFrameworks { get; }

    public Stream GetStream() {
      throw new NotImplementedException();
    }

    public string Path => AbsolutePath;

    public string EffectivePath => Path;

    public FrameworkName TargetFramework { get; }

    public string Name { get; }
  }
}