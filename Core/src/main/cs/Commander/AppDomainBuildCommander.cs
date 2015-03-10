using System;
using System.Security.Policy;

namespace Bud.Commander {
  public class AppDomainBuildCommander : IBuildCommander {
    private readonly AssemblyBuildCommander AssemblyBuildCommander;
    private readonly AppDomain AppDomain;

    public AppDomainBuildCommander(string buildConfigurationAssemblyFile, string dirOfProjectToBeBuilt) {
      AppDomain = AppDomain.CreateDomain("BuildConfiguration", new Evidence(), new AppDomainSetup {ApplicationBase = AppDomain.CurrentDomain.BaseDirectory});
      AssemblyBuildCommander = (AssemblyBuildCommander) AppDomain.CreateInstanceFromAndUnwrap(BudAssemblies.GetBudCoreAssembly().Location, typeof (AssemblyBuildCommander).FullName);
      AssemblyBuildCommander.LoadBuildConfiguration(buildConfigurationAssemblyFile, dirOfProjectToBeBuilt, Console.Out, Console.Error);
    }

    public object Evaluate(string command) {
      return AssemblyBuildCommander.Evaluate(command);
    }

    public void Dispose() {
      AppDomain.Unload(AppDomain);
    }
  }
}