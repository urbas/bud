using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using Bud.Plugins.Build;
using Bud.Plugins.BuildLoading;

namespace Bud.Commander {
  public class AppDomainBuildCommander : IBuildCommander {
    private readonly AssemblyBuildCommander assemblyBuildCommander;
    private readonly AppDomain appDomain;

    public AppDomainBuildCommander(string buildConfigurationAssemblyFile, string dirOfProjectToBeBuilt) {
      appDomain = AppDomain.CreateDomain("BuildConfiguration", new System.Security.Policy.Evidence(), new AppDomainSetup() { ApplicationBase = AppDomain.CurrentDomain.BaseDirectory });
      assemblyBuildCommander = (AssemblyBuildCommander)appDomain.CreateInstanceFromAndUnwrap(BudAssemblies.GetBudCoreAssembly().Location, typeof(AssemblyBuildCommander).FullName);
      assemblyBuildCommander.LoadBuildConfiguration(buildConfigurationAssemblyFile, dirOfProjectToBeBuilt);
    }

    public string Evaluate(string command) {
      return assemblyBuildCommander.Evaluate(command);
    }

    public void Dispose() {
      AppDomain.Unload(appDomain);
    }
  }
}
