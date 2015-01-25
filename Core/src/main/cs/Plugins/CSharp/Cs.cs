using System;
using Bud.Plugins.Build;
using Bud.Plugins.Deps;
using Bud.Plugins.Projects;

namespace Bud.Plugins.CSharp {
  public static class Cs {
    public static Func<Settings, Settings> Dependency(string packageName, string packageVersion = null, string scope = null) {
      var dependencyProjectKey = ProjectsSettings.ProjectKey(packageName);
      var dependencyBuildTargetKey = CSharpBuildTargetPlugin.MainBuildTargetKey(dependencyProjectKey);
      return Dependencies.AddDependency(
        new InternalDependency(dependencyBuildTargetKey, CSharpBuildTargetPlugin.MainBuildTaskKey(dependencyProjectKey)),
        new ExternalDependency(packageName, packageVersion),
        shouldUseInternalDependency: context => IsMainBuildTargetDefined(context, dependencyBuildTargetKey));
    }

    public static Func<Settings, Settings> Exe(params Func<Settings, Settings>[] plugins) {
      return CSharpBuildTargetPlugin.Init(BuildKeys.Main, plugins);
    }

    public static Func<Settings, Settings> Dll(params Func<Settings, Settings>[] plugins) {
      return CSharpBuildTargetPlugin.Init(BuildKeys.Main, CSharpKeys.AssemblyType.Modify(AssemblyType.Library), plugins.ToSettingsTransform());
    }

    public static Func<Settings, Settings> Test(params Func<Settings, Settings>[] plugins) {
      return CSharpBuildTargetPlugin.Init(BuildKeys.Test, CSharpKeys.AssemblyType.Modify(AssemblyType.Library), plugins.ToSettingsTransform(), AddMainBuildTargetAsDependency);
    }

    private static Settings AddMainBuildTargetAsDependency(Settings settings) {
      var buildTarget = settings.Scope;
      var mainBuildTarget = CSharpBuildTargetPlugin.FindSiblingMainBuildTarget(buildTarget);
      var mainBuildTask = CSharpBuildTargetPlugin.FindSiblingMainBuildTask(buildTarget);
      return settings.AddDependency(new InternalDependency(mainBuildTarget, mainBuildTask), config => IsMainBuildTargetDefined(config, mainBuildTarget));
    }

    private static bool IsMainBuildTargetDefined(IConfig context, Key dependencyBuildTargetKey) {
      return context.IsConfigDefined(CSharpKeys.OutputAssemblyFile.In(dependencyBuildTargetKey));
    }
  }
}