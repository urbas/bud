using Bud.Plugins.Build;
using Bud.Plugins.Projects;

namespace Bud.Plugins.CSharp {
  public static class CSharpSettings {
    public static readonly IPlugin MainBuildTargetToDll = BuildUtils.ApplyToBuildTarget(BuildKeys.Main, CSharpKeys.CSharp, CSharpBuildTargetPlugin.ConvertBuildTargetToDll);

    public static Settings ExeProject(this Settings build, string id, string baseDir, params IPlugin[] plugins) {
      return build.Project(id, baseDir, CSharpPlugin.Instance.With(plugins));
    }

    public static Settings DllProject(this Settings build, string id, string baseDir, params IPlugin[] plugins) {
      return build.ExeProject(id, baseDir, MainBuildTargetToDll.With(plugins));
    }
  }
}