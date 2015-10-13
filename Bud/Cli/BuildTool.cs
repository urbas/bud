using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using Bud.Compilation;
using static System.IO.Directory;
using static System.IO.Path;
using static Bud.Build;
using static Bud.IO.FilesObservatory;

namespace Bud.Cli {
  public class BuildTool {
    public static void Main(string[] args) {
      var compilationOutput = CSharp.CSharpProject(Combine(GetCurrentDirectory(), "..", "..", "..", "Bud"))
                                    .Add(BudDependencies())
                                    .Set(Sources, configs => FilesObservatory[configs].ObserveFileList(Combine(ProjectDir[configs], "Bud.cs")))
                                    .Get(CSharp.Compilation)
                                    .ToEnumerable()
                                    .First();
      if (compilationOutput.Success) {
        var assembly = Assembly.LoadFile(compilationOutput.AssemblyPath);
        var buildDefinitionType = assembly.GetExportedTypes().Single(typeof(IBuild).IsAssignableFrom);
        var buildDefinition = (IBuild) buildDefinitionType.GetConstructor(Type.EmptyTypes).Invoke(new object[] {});
        var configs = buildDefinition.Init(GetCurrentDirectory());
        configs.Get<int>("hello");
        Console.WriteLine($"Success!!");
      }
      Console.ReadLine();
    }

    private static Configs BudDependencies()
      => Configs.Empty.Set(CSharp.Dependencies, c => FilesObservatory[c].ObserveAssemblies(
        typeof(BuildTool).Assembly.Location,
        typeof(object).Assembly.Location));
  }
}