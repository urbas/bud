using Bud.Plugins.Dependencies;

namespace Bud.Plugins.NuGet {
  public class NuGetPlugin : IPlugin {

    public static readonly NuGetPlugin Instance = new NuGetPlugin();

    private NuGetPlugin() {
    }

    public Settings ApplyTo(Settings settings, Scope scope) {
      return settings
        .Add(DependenciesPlugin.Instance)
        .InitOrKeep(NuGetKeys.NuGetDependencyResolver, context => new NuGetDependencyResolver());
    }
  }

}

