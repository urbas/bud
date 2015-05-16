using System.Collections.Generic;
using System.IO;
using System.Runtime.Versioning;
using Bud.Build;
using NuGet;

namespace Bud.CSharp {
  public class CSharpBuildTargetAssembly : IPackageAssemblyReference {
    private readonly Framework targetFramework;

    public CSharpBuildTargetAssembly(IConfig config, Key buildTarget) {
      BuildTarget = buildTarget;
      Path = config.GetCSharpOutputAssemblyFile(buildTarget);
      targetFramework = config.GetTargetFramework(buildTarget);
      Name = config.GetCSharpOutputAssemblyName(buildTarget);
      Id = config.PackageIdOf(buildTarget);
    }

    public string Id { get; }

    public Key BuildTarget { get; }

    public IEnumerable<FrameworkName> SupportedFrameworks => null;

    public Stream GetStream() => new FileStream(EffectivePath, FileMode.Open);

    public string Path { get; }

    public string EffectivePath => Path;

    public FrameworkName TargetFramework => targetFramework.FrameworkName;

    public string Name { get; }

    public IPackageFile ToPackagedAssembly() {
      return new PhysicalPackageFile {
        TargetPath = $"lib/{targetFramework.Identifier}/{System.IO.Path.GetFileName(Path)}",
        SourcePath = Path
      };
    }
  }
}