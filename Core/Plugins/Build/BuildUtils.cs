using Bud.Plugins.CSharp;

namespace Bud.Plugins.Build {
  public static class BuildUtils {
    public static Key BuildTargetKey(Key project, Key scope, Key language) {
      return language.In(scope.In(project));
    }

    public static TaskKey<Unit> BuildTaskKey(Key project, Key scope, Key language) {
      return BuildKeys.Build.In(BuildTargetKey(project, scope, language));
    }
  }
}