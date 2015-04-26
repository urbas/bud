using NuGet;

namespace Bud.Dependencies {
  public interface IDependency {
    IPackage AsPackage(IConfig config);
  }
}