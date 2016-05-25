using System;

namespace Bud.FrameworkAssemblies {
  public struct FrameworkDir {
    public Version Version { get; }
    public string Dir { get; }

    public FrameworkDir(Version version, string dir) {
      Version = version;
      Dir = dir;
    }
  }
}