using System;
using System.Collections.Generic;
using System.Linq;
using static System.IO.File;
using static System.IO.Path;

namespace Bud.NuGet {
  public class FrameworkAssemblyResolver {
    public static readonly string ReferenceAssembliesPath = Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Reference Assemblies", "Microsoft", "Framework", ".NETFramework");

    public static IEnumerable<string> ResolveFrameworkAsseblies(IEnumerable<FrameworkAssemblyReference> frameworkAssemblies)
      => frameworkAssemblies.Select(ResolveFrameworkAssembly);

    private static string ResolveFrameworkAssembly(FrameworkAssemblyReference reference) {
      var rootReferenceAssemblyPath = ToRootReferenceAssemblyPath(reference);
      return Exists(rootReferenceAssemblyPath) ?
        rootReferenceAssemblyPath :
        ToFacadeReferenceAssemblyPath(reference);
    }

    private static string ToRootReferenceAssemblyPath(FrameworkAssemblyReference reference)
      => Combine(ReferenceAssembliesPath, ToDir(reference.Framework.Version), $"{reference.AssemblyName}.dll");

    private static string ToFacadeReferenceAssemblyPath(FrameworkAssemblyReference reference)
      => Combine(ReferenceAssembliesPath, ToDir(reference.Framework.Version), "Facades", $"{reference.AssemblyName}.dll");

    private static string ToDir(Version version)
      => version.Build == 0 ?
        $"v4.{version.Minor}" :
        $"v4.{version.Minor}.{version.Build}";
  }
}