using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NuGet;

namespace Bud {
  public static class BudAssemblies {
    private const string ImmutableCollectionsDependency = "System.Collections.Immutable";

    public static IEnumerable<Assembly> AllAssemblies { get; } = AppDomain.CurrentDomain
                                                                          .GetAssemblies()
                                                                          .Where(IsBudAssembly);

    public static Assembly CoreAssembly { get; } = AllAssemblies.Single(IsCoreAssembly);

    public static IEnumerable<Assembly> CoreDependencies { get; } = AppDomain.CurrentDomain
                                                                             .GetAssemblies()
                                                                             .Where(IsImmutableCollectionsAssembly);

    public static IEnumerable<IPackageAssemblyReference> AssemblyReferences { get; } = AllAssemblies.Select(ToAssembyReference);

    public static IEnumerable<IPackageAssemblyReference> CoreDependenciesReferences { get; } = CoreDependencies.Select(ToAssembyReference);

    private static bool IsImmutableCollectionsAssembly(Assembly assembly) {
      return ImmutableCollectionsDependency.Equals(assembly.GetName().Name);
    }

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