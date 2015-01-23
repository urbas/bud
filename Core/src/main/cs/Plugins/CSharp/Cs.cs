using Bud.Plugins.Build;
using Bud.Plugins.Deps;
using Bud.Plugins.Projects;

namespace Bud.Plugins.CSharp {
  public static class Cs {
    public static IPlugin Dependency(string packageName, string packageVersion = null, string scope = null) {
      var dependencyProjectKey = ProjectsSettings.ProjectKey(packageName);
      var dependencyBuildTargetKey = CSharpBuildTargetPlugin.MainBuildTargetKey(dependencyProjectKey);
      return Dependencies.AddDependency(
        new InternalDependency(dependencyBuildTargetKey, CSharpBuildTargetPlugin.MainBuildTaskKey(dependencyProjectKey)),
        new ExternalDependency(packageName, packageVersion),
        shouldUseInternalDependency: context => IsMainBuildTargetDefined(context, dependencyBuildTargetKey));
    }

    private static bool IsMainBuildTargetDefined(IConfig context, Key dependencyBuildTargetKey) {
      return context.IsConfigDefined(CSharpKeys.OutputAssemblyFile.In(dependencyBuildTargetKey));
    }

    public static IPlugin Exe(params IPlugin[] plugins) {
      return new CSharpBuildTargetPlugin(BuildKeys.Main, plugins);
    }

    public static IPlugin Dll(params IPlugin[] plugins) {
      return new CSharpBuildTargetPlugin(BuildKeys.Main, CSharpBuildTargetPlugin.ConvertBuildTargetToDll.With(plugins));
    }

    public static IPlugin Test(params IPlugin[] plugins) {
      return new CSharpBuildTargetPlugin(BuildKeys.Test, CSharpBuildTargetPlugin.ConvertBuildTargetToDll.With(plugins).With(AddMainBuildTargetAsDependency));
    }

    private static Settings AddMainBuildTargetAsDependency(Settings settings, Key buildTarget) {
      var mainBuildTarget = CSharpBuildTargetPlugin.FindSiblingMainBuildTarget(buildTarget);
      var mainBuildTask = CSharpBuildTargetPlugin.FindSiblingMainBuildTask(buildTarget);
      return settings.AddDependency(buildTarget, new InternalDependency(mainBuildTarget, mainBuildTask), config => IsMainBuildTargetDefined(config, mainBuildTarget));
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