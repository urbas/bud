using static System.IO.File;
using static System.IO.Path;
using static Bud.BuildDefinition.BuildDefinitionSettings;
using Bud.Build;
using Bud.BuildDefinition;
using Bud.CSharp;
using Newtonsoft.Json;

namespace Bud.Commander {
  public static class BuildCommander {
    public static IBuildCommander LoadBuildCommander(int buildLevel, bool isQuiet, string currentProjectBaseDir) {
      if (buildLevel <= 0) {
        return LoadProjectLevelCommander(currentProjectBaseDir, isQuiet);
      }
      // TODO: descend to the target build level first (i.e., go to the directory .bud/.../.bud/)
      return LoadBuildLevelCommander(BudDir(currentProjectBaseDir), isQuiet);
    }

    /// <summary>
    ///   This method returns a build commander whose tasks are defined in <c>projectBaseDir/.bud/Build.cs</c>.
    /// </summary>
    public static IBuildCommander LoadProjectLevelCommander(string projectBaseDir, bool isQuiet) {
      BuildDefinitionInfo buildDefinitionInfo;
      if (TryLoadBuildDefinition(BudDir(projectBaseDir), out buildDefinitionInfo, isQuiet)) {
        return new AppDomainBuildCommander(buildDefinitionInfo, projectBaseDir, BuildCommanderType.ProjectLevel, isQuiet);
      }
      return new DefaultBuildCommander(projectBaseDir, isQuiet);
    }

    /// <summary>
    ///   This method returns <c>true</c> if a build definition assembly has been produced,
    ///   in which case the build definition assembly file is set to a valid path.
    ///   Otherwise, when this method returns <c>false</c>, the build assembly
    ///   file will be set to <c>null</c>.
    /// </summary>
    private static bool TryLoadBuildDefinition(string budDir, out BuildDefinitionInfo buildDefinitionInfo, bool isQuiet) {
      if (Exists(BuildDefinitionSourceFile(budDir))) {
        using (var buildLevelCommander = LoadBuildLevelCommander(budDir, isQuiet)) {
          buildLevelCommander.EvaluateToJson(BuildKeys.Build);
          buildDefinitionInfo = new BuildDefinitionInfo(
            BuildDefinitionAssemblyFile(budDir),
            JsonConvert.DeserializeObject<string[]>(buildLevelCommander.EvaluateToJson(BuildDefinitionPlugin.BuildDefinitionProjectKey / BuildKeys.Main / Cs.CSharp / Cs.AssemblyReferencePaths)));
        }
        return true;
      }
      buildDefinitionInfo = null;
      return false;
    }

    private static IBuildCommander LoadBuildLevelCommander(string projectToBuildDir, bool isQuiet) {
      BuildDefinitionInfo buildDefinitionInfo;
      if (TryLoadBuildDefinition(BudDir(projectToBuildDir), out buildDefinitionInfo, isQuiet)) {
        return new AppDomainBuildCommander(buildDefinitionInfo, projectToBuildDir, BuildCommanderType.BuildLevel, isQuiet);
      }
      return new DefaultBuildCommander(DefaultBuildDefinitionProject(projectToBuildDir), isQuiet);
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