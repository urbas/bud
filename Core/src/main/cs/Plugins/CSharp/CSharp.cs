using Bud.Plugins.Build;
using Bud.Plugins.Deps;
using Bud.Plugins.Projects;

namespace Bud.Plugins.CSharp {
  public static class CSharp {
    public static IPlugin Dependency(string packageName, string packageVersion = null, string scope = null) {
      var dependencyProjectKey = Project.ProjectKey(packageName);
      var dependencyBuildTargetKey = CSharpBuildTargetPlugin.MainBuildTargetKey(dependencyProjectKey);
      return ApplyTo(scope, Dependencies.AddDependency(
        new InternalDependency(dependencyBuildTargetKey, CSharpBuildTargetPlugin.MainBuildTaskKey(dependencyProjectKey)),
        new ExternalDependency(packageName, packageVersion),
        shouldUseInternalDependency: context => context.IsConfigDefined(CSharpKeys.OutputAssemblyFile.In(dependencyBuildTargetKey))));
    }

    public static IPlugin TargetFramework(Framework framework, string scope = null) {
      return ApplyTo(scope, (settings, buildTarget) => settings.Modify(CSharpKeys.TargetFramework.In(buildTarget), config => framework));
    }

    private static IPlugin ApplyTo(string scope, SettingsTransform settingsTransform) {
      return ApplyTo(scope, PluginUtils.Create(settingsTransform));
    }

    private static IPlugin ApplyTo(string scope, IPlugin addDependency) {
      return PluginUtils.ApplyToSubKey(GetBuildTargetOrMain(scope), addDependency);
    }

    private static Key GetBuildTargetOrMain(string target) {
      return CSharpKeys.CSharp.In(target == null ? BuildKeys.Main : Key.Parse(target));
    }
  }
}