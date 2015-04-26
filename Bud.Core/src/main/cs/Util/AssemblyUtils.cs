using System;
using System.Linq;
using System.Reflection;

namespace Bud.Util {
  public static class AssemblyUtils {
    public static bool IsPathOfAssembly(string assemblyPath, AssemblyName assemblyName) {
      return assemblyPath.EndsWith(assemblyName.Name + ".dll");
    }

    public static ResolveEventHandler PathListAssemblyResolver(string[] dependencyDlls) {
      return (sender, args) => {
        var assemblyName = new AssemblyName(args.Name);
        var foundAssemblyPath = dependencyDlls.FirstOrDefault(assemblyPath => AssemblyUtils.IsPathOfAssembly(assemblyPath, assemblyName));
        return foundAssemblyPath == null ? null : Assembly.LoadFrom(foundAssemblyPath);
      };
    }
  }
}