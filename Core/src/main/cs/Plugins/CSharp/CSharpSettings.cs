using Bud.Plugins.Build;
using Bud.Plugins.Projects;

namespace Bud.Plugins.CSharp {
  public static class CSharpSettings {
    public static readonly IPlugin MainBuildTargetToDll = BuildUtils.ApplyToBuildTarget(BuildKeys.Main, CSharpKeys.CSharp, CSharpBuildTargetPlugin.ConvertBuildTargetToDll);

    public static Settings DllProjectWithoutTests(this Settings build, string id, string baseDir, params IPlugin[] plugins) {
      return build.AddProject(id, baseDir, new CSharpPlugin(false).With(MainBuildTargetToDll).With(plugins));
    }

    public static Settings ExeProject(this Settings build, string id, string baseDir, params IPlugin[] plugins) {
      return build.AddProject(id, baseDir, CSharpPlugin.Instance.With(plugins));
    }

    public static Settings DllProject(this Settings build, string id, string baseDir, params IPlugin[] plugins) {
      return build.ExeProject(id, baseDir, MainBuildTargetToDll.With(plugins));
    }
  }
}