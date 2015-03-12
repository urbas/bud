using System.Collections.Generic;
using System.Linq;
using Bud.CSharp;
using NuGet;

namespace Bud.Commander {
  public static class BuildCommanderPlugin {
    public static Setup BudAssemblyReferences => CSharpKeys.AssemblyReferences.Modify(AddBudAssembliesImpl);

    private static IEnumerable<IPackageAssemblyReference> AddBudAssembliesImpl(IConfig config, IEnumerable<IPackageAssemblyReference> existingAssemblies) {
      return existingAssemblies.Concat(BudAssemblies.GetBudAssemblyReferences());
    }
  }
}