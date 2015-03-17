namespace Bud.BuildDefinition {
  public static class BuildDefinitionConfig {
    public static string GetBuildConfigSourceFile(this IContext context, Key buildLoadingProject) {
      return context.Evaluate(buildLoadingProject / BuildDefinitionKeys.BuildConfigSourceFile);
    }
  }
}