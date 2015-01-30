namespace Bud.Build {
  public static class BuildUtils {
    /// <returns>
    ///   key corresponds to the build of sources in the folder '[project]/src/[scope]/[language]'.
    /// </returns>
    public static Key BuildTarget(Key project, Key scope, Key language) {
      return language.In(scope.In(project));
    }

    /// <returns>
    ///   key corresponds to the task that builds the sources in the folder '[project]/src/[scope]/[language]'.
    /// </returns>
    public static TaskKey BuildTaskKey(Key project, Key scope, Key language) {
      return BuildKeys.Build.In(BuildTarget(project, scope, language));
    }

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