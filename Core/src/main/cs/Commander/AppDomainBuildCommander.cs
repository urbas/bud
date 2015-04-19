using System;
using System.Security.Policy;

namespace Bud.Commander {
  public class AppDomainBuildCommander : IBuildCommander {
    private const string BuildConfigurationAppDomainName = "BuildConfiguration";
    private readonly AssemblyBuildCommander AssemblyBuildCommander;
    private readonly AppDomain AppDomain;

    public AppDomainBuildCommander(BuildDefinitionInfo buildDefinitionInfo, string dirOfProjectToBeBuilt, int buildLevel) {
      AppDomain = AppDomain.CreateDomain(BuildConfigurationAppDomainName, new Evidence(), new AppDomainSetup {ApplicationBase = AppDomain.CurrentDomain.BaseDirectory});
      try {
        AssemblyBuildCommander = (AssemblyBuildCommander) AppDomain.CreateInstanceFromAndUnwrap(BudAssemblies.CoreAssembly.Location, typeof(AssemblyBuildCommander).FullName);
        AssemblyBuildCommander.LoadBuildDefinition(buildDefinitionInfo.BuildDefinitionAssemblyFile,
                                                   buildDefinitionInfo.DependencyDlls,
                                                   dirOfProjectToBeBuilt,
                                                   buildLevel,
                                                   Console.Out,
                                                   Console.Error);
      } catch (Exception) {
        Dispose();
        throw;
      }
    }

    public string EvaluateToJson(string command) {
      return AssemblyBuildCommander.EvaluateToJson(command);
    }

    public void Dispose() {
      try {
        AssemblyBuildCommander.Dispose();
      } finally {
        AppDomain.Unload(AppDomain);
      }
    }
  }
}