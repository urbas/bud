using System.Collections.Generic;
using System.IO;
using System.Runtime.Versioning;
using NuGet;

namespace Bud.CSharp {
  public class CSharpBuildTargetAssembly : IPackageAssemblyReference {
    private readonly string path;
    private readonly Framework targetFramework;
    private readonly string name;

    public CSharpBuildTargetAssembly(IConfig config, Key buildTarget) {
      path = config.GetCSharpOutputAssemblyFile(buildTarget);
      targetFramework = config.GetTargetFramework(buildTarget);
      name = config.GetCSharpOutputAssemblyName(buildTarget);
    }

    public IEnumerable<FrameworkName> SupportedFrameworks {
      get { return null; }
    }

    public Stream GetStream() {
      return new FileStream(EffectivePath, FileMode.Open);
    }

    public string Path {
      get { return path; }
    }

    public string EffectivePath {
      get { return path; }
    }

    public FrameworkName TargetFramework {
      get { return targetFramework.FrameworkName; }
    }

    public string Name {
      get { return name; }
    }

    public IPackageFile ToPackagedAssembly() {
      return new PhysicalPackageFile {
        TargetPath = string.Format("lib/{0}/{1}", targetFramework.Identifier, System.IO.Path.GetFileName(Path)),
        SourcePath = Path
      };
    }
  }
}