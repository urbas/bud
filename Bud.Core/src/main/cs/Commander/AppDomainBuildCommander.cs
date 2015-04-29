using System;
using System.Security.Policy;
using Bud.BuildDefinition;
using Bud.IO;

namespace Bud.Commander {
  public class AppDomainBuildCommander : IBuildCommander {
    private const string BuildConfigurationAppDomainName = "BuildConfiguration";
    private readonly AssemblyBuildCommander AssemblyBuildCommander;
    private readonly AppDomain AppDomain;

    public AppDomainBuildCommander(BuildDefinitionInfo buildDefinitionInfo, string dirOfProjectToBeBuilt, int buildLevel, bool isQuiet) {
      AppDomain = AppDomain.CreateDomain(BuildConfigurationAppDomainName, new Evidence(), new AppDomainSetup {ApplicationBase = AppDomain.CurrentDomain.BaseDirectory});
      try {
        AssemblyBuildCommander = (AssemblyBuildCommander) AppDomain.CreateInstanceFromAndUnwrap(BudAssemblies.CoreAssembly.Location, typeof(AssemblyBuildCommander).FullName);
        AssemblyBuildCommander.LoadBuildDefinition(buildDefinitionInfo.BuildDefinitionAssemblyFile,
                                                   buildDefinitionInfo.DependencyDlls,
                                                   dirOfProjectToBeBuilt,
                                                   buildLevel,
                                                   isQuiet ? NullTextWriter.Instance : Console.Out,
                                                   isQuiet ? NullTextWriter.Instance : Console.Error);
      } catch (Exception) {
        Dispose();
        throw;
      }
    }

    public string EvaluateToJson(string command) => AssemblyBuildCommander.EvaluateToJson(command);

    public string EvaluateMacroToJson(string macroName, params string[] commandLineParameters) => AssemblyBuildCommander.EvaluateMacroToJson(macroName, commandLineParameters);

    public void Dispose() {
      try {
        AssemblyBuildCommander.Dispose();
      } finally {
        AppDomain.Unload(AppDomain);
      }
    }
  }
}