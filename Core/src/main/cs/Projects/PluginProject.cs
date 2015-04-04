using Bud.BuildDefinition;
using Bud.CSharp;

namespace Bud.Projects {
  public class PluginProject : Project {
    public PluginProject(string id, string baseDir, params Setup[] setups)
      : base(id, baseDir, Cs.Dll(BuildDefinitionPlugin.BudAssemblyReferences, setups.Merge())) {}
  }
}