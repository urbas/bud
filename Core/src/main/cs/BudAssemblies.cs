using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NuGet;

namespace Bud {
  public static class BudAssemblies {
    public static IEnumerable<string> GetBudAssembliesLocations() {
      return GetBudAssemblies().Select(assembly => assembly.Location);
    }

    public static IEnumerable<Assembly> GetBudAssemblies() {
      return AppDomain.CurrentDomain.GetAssemblies()
                      .Where(assembly => assembly.GetName().Name.StartsWith("Bud."));
    }

    public static Assembly GetBudCoreAssembly() {
      return GetBudAssemblies()
        .First(assembly => assembly.GetName().Name.EndsWith(".Core"));
    }

    public static IEnumerable<IPackageAssemblyReference> GetBudAssemblyReferences() {
      return GetBudAssemblies()
        .Select(assembly => new PhysicalPackageAssemblyReference {SourcePath = assembly.Location, TargetPath = assembly.Location});
    }
  }
}