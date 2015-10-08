using static Bud.Configs;

namespace Bud {
  public static class NuGet {
    public readonly static Key<string> PackagesFile = nameof(PackagesFile);

    public static Configs NuGetPackages()
      => Empty.Init(PackagesFile, null);
  }
}