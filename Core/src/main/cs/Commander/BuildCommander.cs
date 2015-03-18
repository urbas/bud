using System.IO.File;
using System.IO.Path;
using Bud.Build;
using Bud.BuildDefinition;
using Bud.BuildDefinition.BuildDefinitionSettings;
using Bud.CSharp;

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
      BuildDefinitionInfo buildDefinitionInfo;
      if (TryLoadBuildDefinition(BudDir(projectBaseDir), out buildDefinitionInfo)) {
        return new AppDomainBuildCommander(buildDefinitionInfo, projectBaseDir, BuildCommanderType.ProjectLevel);
      }
      return new DefaultBuildCommander(projectBaseDir);
    }

    /// <summary>
    ///   This method returns <c>true</c> if a build definition assembly has been produced,
    ///   in which case the build definition assembly file is set to a valid path.
    ///   Otherwise, when this method returns <c>false</c>, the build assembly
    ///   file will be set to <c>null</c>.
    /// </summary>
    private static bool TryLoadBuildDefinition(string budDir, out BuildDefinitionInfo buildDefinitionInfo) {
      if (Exists(BuildDefinitionSourceFile(budDir))) {
        using (var buildLevelCommander = LoadBuildLevelCommander(budDir)) {
          // TODO: Do not fetch dependencies if they were fetched already.
          // This call must be super quick.
//          thisBuildLevelCommander.Evaluate(DependenciesKeys.FetchDependencies);
          buildLevelCommander.Evaluate(BuildKeys.Build);
          buildDefinitionInfo = new BuildDefinitionInfo(
            BuildDefinitionAssemblyFile(budDir),
            (string[]) buildLevelCommander.Evaluate(BuildDefinitionPlugin.BuildDefinitionProjectKey / BuildKeys.Main / CSharpKeys.CSharp / CSharpKeys.AssemblyReferencePaths)
            );
        }
        return true;
      }
      buildDefinitionInfo = null;
      return false;
    }

    private static IBuildCommander LoadBuildLevelCommander(string projectToBuildDir) {
      BuildDefinitionInfo buildDefinitionInfo;
      if (TryLoadBuildDefinition(BudDir(projectToBuildDir), out buildDefinitionInfo)) {
        return new AppDomainBuildCommander(buildDefinitionInfo, projectToBuildDir, BuildCommanderType.BuildLevel);
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