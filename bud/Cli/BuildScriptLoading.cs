using System;
using System.Linq;
using System.Reflection;
using Bud.V1;

namespace Bud.Cli {
  public static class BuildScriptLoading {
    public static IConf LoadBuildDefinition(string assemblyPath, string baseDir)
      => LoadBuildDefinition(LoadBuildConf(assemblyPath), baseDir);

    internal static IConf LoadBuildDefinition(Conf buildConf, string baseDir)
      => buildConf.InitValue(Api.BaseDir, baseDir)
                  .ToCompiled();

    private static Conf LoadBuildConf(string assemblyPath) {
      var assembly = Assembly.LoadFile(assemblyPath);
      var buildDefinitionType = assembly
        .GetExportedTypes()
        .First(typeof(IBuild).IsAssignableFrom);
      var buildDefinition = buildDefinitionType
        .GetConstructor(Type.EmptyTypes)
        .Invoke(new object[] {});
      return ((IBuild) buildDefinition).Init();
    }
  }
}