using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Antlr4.StringTemplate;
using CommandLine;
using Newtonsoft.Json;
using NuGet;

namespace Bud.BuildDefinition {
  public static class BudAssemblies {
    static BudAssemblies() {}

    public static Assembly CoreAssembly { get; } = typeof(Settings).Assembly;

    public static IEnumerable<IPackageAssemblyReference> AssemblyReferences { get; }
      = ImmutableList.Create(ToAssembyReference(CoreAssembly));

    public static IEnumerable<IPackageAssemblyReference> CoreDependenciesReferences { get; }
      = ImmutableList.Create(ToAssembyReference(typeof(Parser).Assembly),
                             ToAssembyReference(typeof(JsonConvert).Assembly),
                             ToAssembyReference(typeof(PackageManager).Assembly),
                             ToAssembyReference(typeof(Template).Assembly),
                             ToAssembyReference(typeof(ImmutableList).Assembly));

    private static PhysicalPackageAssemblyReference ToAssembyReference(Assembly assembly) {
      return new PhysicalPackageAssemblyReference {SourcePath = assembly.Location, TargetPath = assembly.Location};
    }
  }
}