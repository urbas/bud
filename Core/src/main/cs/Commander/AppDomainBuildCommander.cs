using System;
using System.Security.Policy;

namespace Bud.Commander {
  public class AppDomainBuildCommander : IBuildCommander {
    private const string BuildConfigurationAppDomainName = "BuildConfiguration";
    private readonly AssemblyBuildCommander AssemblyBuildCommander;
    private readonly AppDomain AppDomain;

    public AppDomainBuildCommander(string buildConfigurationAssemblyFile, string dirOfProjectToBeBuilt, int buildType) {
      AppDomain = AppDomain.CreateDomain(BuildConfigurationAppDomainName, new Evidence(), new AppDomainSetup {ApplicationBase = AppDomain.CurrentDomain.BaseDirectory});
      try {
        AssemblyBuildCommander = (AssemblyBuildCommander) AppDomain.CreateInstanceFromAndUnwrap(BudAssemblies.CoreAssembly.Location, typeof (AssemblyBuildCommander).FullName);
        AssemblyBuildCommander.LoadBuildConfiguration(buildConfigurationAssemblyFile, dirOfProjectToBeBuilt, buildType, Console.Out, Console.Error);
      } catch (Exception) {
        Dispose();
        throw;
      }
    }

    public object Evaluate(string command) {
      return AssemblyBuildCommander.Evaluate(command);
    }

    public void Dispose() {
      AssemblyBuildCommander.Dispose();
      AppDomain.Unload(AppDomain);
    }
  }
}