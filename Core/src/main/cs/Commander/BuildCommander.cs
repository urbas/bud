using System;
using System.IO.File;
using System.IO.Path;
using Bud.Build;
using Bud.BuildDefinition;
using Bud.BuildDefinition.BuildDefinitionSettings;
using Bud.CSharp;
using Bud.Dependencies;
using Bud.Projects;

namespace Bud.Commander {
  public static class BuildCommander {
    public static IBuildCommander LoadBuildCommander(int buildLevel,
                                                     string currentProjectBaseDir) {
      if (buildLevel <= 0) {
        return LoadProjectLevelCommander(currentProjectBaseDir);
      }
      // TODO: descend to the target build level first (i.e., go to the directory .bud/.../.bud/)
      return LoadBuildLevelCommander(BudDir(currentProjectBaseDir));
    }

    /// <summary>
    ///   This method returns a build commander whose tasks are defined in <c>projectBaseDir/.bud/Build.cs</c>.
    /// </summary>
    public static IBuildCommander LoadProjectLevelCommander(string projectBaseDir) {
      string buildDefinitionAssemblyFile;
      if (TryCompileBuildDefinition(BudDir(projectBaseDir), out buildDefinitionAssemblyFile)) {
        return new AppDomainBuildCommander(buildDefinitionAssemblyFile, projectBaseDir, BuildCommanderType.ProjectLevel);
      }
      return new DefaultBuildCommander(projectBaseDir);
    }

    /// <summary>
    /// This method returns <c>true</c> if a build definition assembly has been produced,
    /// in which case the build definition assembly file is set to a valid path.
    /// Otherwise, when this method returns <c>false</c>, the build assembly
    /// file will be set to <c>null</c>.
    /// </summary>
    private static bool TryCompileBuildDefinition(string budDir, out string buildDefinitionAssemblyFile) {
      if (Exists(BuildDefinitionSourceFile(budDir))) {
        using (var buildLevelCommander = LoadBuildLevelCommander(budDir)) {
          // TODO: Do not fetch dependencies if they were fetched already.
          // This call must be super quick.
//          thisBuildLevelCommander.Evaluate(DependenciesKeys.FetchDependencies);
          // TODO: Reset the commander so that it has the fetched dependencies when they are
          // fetched for the first time.
//          thisBuildLevelCommander.Evaluate(ProjectPlugin.ProjectKey(BuildDefinitionPlugin.BuildDefinitionProjectId) / BuildKeys.Main / CSharpKeys.CSharp / CSharpKeys.Dist);
          buildLevelCommander.Evaluate(BuildKeys.Build);
        }
        buildDefinitionAssemblyFile = BuildDefinitionAssemblyFile(budDir);
        return true;
      }
      buildDefinitionAssemblyFile = null;
      return false;
    }

    private static IBuildCommander LoadBuildLevelCommander(string projectToBuildDir) {
      string buildDefinitionAssemblyFile;
      if (TryCompileBuildDefinition(BudDir(projectToBuildDir), out buildDefinitionAssemblyFile)) {
        return new AppDomainBuildCommander(buildDefinitionAssemblyFile, projectToBuildDir, BuildCommanderType.BuildLevel);
      }
      return new DefaultBuildCommander(DefaultBuildDefinitionProject(projectToBuildDir));
    }

    private static string BuildDefinitionAssemblyFile(string budDir) {
      return Combine(budDir, BuildDefinitionPlugin.BuildDefinitionAssemblyFileName);
    }

    private static string BuildDefinitionSourceFile(string budDir) {
      return Combine(budDir, BuildDefinitionPlugin.BuildDefinitionSourceFileName);
    }

    private static string BudDir(string projectBaseDir) {
      return Combine(projectBaseDir, BudPaths.BudDirName);
    }
  }
}