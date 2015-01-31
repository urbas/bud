namespace Bud.Build {
  public static class BuildUtils {
    public static Key ProjectOf(Key buildTarget) {
      return buildTarget.Parent.Parent;
    }

    private static Key ScopeOf(Key buildTarget) {
      return buildTarget.Parent;
    }

    public static string IdOf(Key buildTarget) {
      var projectId = ProjectOf(buildTarget).Id;
      var scope = ScopeOf(buildTarget);
      return scope.IdsEqual(BuildKeys.Main) ? projectId : projectId + "." + scope;
    }
  }
}