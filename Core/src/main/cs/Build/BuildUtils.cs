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
  }
}