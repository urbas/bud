using System;

namespace Bud.Commander {
  public class BuildDefinitionInfo : MarshalByRefObject {
    public BuildDefinitionInfo(string buildDefinitionAssemblyFile, string[] dependencyDlls) {
      BuildDefinitionAssemblyFile = buildDefinitionAssemblyFile;
      DependencyDlls = dependencyDlls;
    }

    public string BuildDefinitionAssemblyFile { get; }

    public string[] DependencyDlls { get; }
  }
}