using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NuGet;

namespace Bud {
  public static class BudAssemblies {
    public static IEnumerable<Assembly> AllAssemblies { get; } = AppDomain.CurrentDomain
                                                                          .GetAssemblies()
                                                                          .Where(IsBudAssembly);

    public static Assembly CoreAssembly { get; } = AllAssemblies.Single(IsCoreAssembly);

    public static IEnumerable<IPackageAssemblyReference> AssemblyReferences { get; } = AllAssemblies.Select(ToAssembyReference);

    private static bool IsBudAssembly(Assembly assembly) {
      return assembly.GetName().Name.StartsWith("Bud.");
    }

    private static bool IsCoreAssembly(Assembly assembly) {
      return assembly.GetName().Name.EndsWith(".Core");
    }

    private static PhysicalPackageAssemblyReference ToAssembyReference(Assembly assembly) {
      return new PhysicalPackageAssemblyReference {SourcePath = assembly.Location, TargetPath = assembly.Location};
    }
  }
}