using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Bud.Plugins.Build;
using Bud.Plugins.Deps;
using Bud.Plugins.Projects;

namespace Bud.Plugins.CSharp {
  public static class CSharp {
    public static readonly IPlugin MainBuildTargetToDll = BuildUtils.ApplyToBuildTarget(BuildKeys.Main, CSharpKeys.CSharp, CSharpBuildPlugin.ConvertBuildTargetToDll);

    public static Settings ExeProject(this Settings build, string id, string baseDir, params IPlugin[] plugins) {
      return build.AddProject(id, baseDir, CSharpPlugin.Instance.With(plugins));
    }

    public static Settings DllProject(this Settings build, string id, string baseDir, params IPlugin[] plugins) {
      return build.ExeProject(id, baseDir, MainBuildTargetToDll.With(plugins));
    }

    public static IPlugin Dependency(string packageName, string packageVersion = null) {
      var projectKey = Project.ProjectKey(packageName);
      var buildTargetKey = MainBuildTargetKey(projectKey);
      return PluginUtils.ApplyToSubKey(
        CSharpKeys.CSharp.In(BuildKeys.Main),
        Dependencies.AddDependency(
          new InternalDependency(buildTargetKey, MainBuildTaskKey(projectKey)),
          new ExternalDependency(packageName, packageVersion),
          shouldUseInternalDependency: context => context.IsConfigDefined(CSharpKeys.OutputAssemblyFile.In(buildTargetKey)))
        );
    }

    public static Task<IEnumerable<string>> GetCSharpSources(this IContext context, Key project) {
      return context.Evaluate(CSharpKeys.SourceFiles.In(project));
    }

    public static string GetCSharpOutputAssemblyDir(this IConfig context, Key key) {
      return context.Evaluate(CSharpKeys.OutputAssemblyDir.In(key));
    }

    public static string GetCSharpOutputAssemblyName(this IConfig context, Key key) {
      return context.Evaluate(CSharpKeys.OutputAssemblyName.In(key));
    }

    public static string GetCSharpOutputAssemblyFile(this IConfig context, Key key) {
      return context.Evaluate(CSharpKeys.OutputAssemblyFile.In(key));
    }

    public static Task<ImmutableList<string>> CollectCSharpReferencedAssemblies(this IContext context, Key project) {
      return context.Evaluate(CSharpKeys.CollectReferencedAssemblies.In(project));
    }

    public static AssemblyType GetCSharpAssemblyType(this IConfig context, Key project) {
      return context.Evaluate(CSharpKeys.AssemblyType.In(project));
    }

    public static Task CSharpBuild(this IContext context, Key project) {
      return context.Evaluate(BuildUtils.BuildTaskKey(project, BuildKeys.Main, CSharpKeys.CSharp));
    }

    public static Key MainBuildTargetKey(Key project) {
      return BuildUtils.BuildTargetKey(project, BuildKeys.Main, CSharpKeys.CSharp);
    }

    public static TaskKey<Unit> MainBuildTaskKey(Key projectKey) {
      return BuildUtils.BuildTaskKey(projectKey, BuildKeys.Main, CSharpKeys.CSharp);
    }
  }
}