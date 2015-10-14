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
      var compilationOutput = CSharp.CSharpProject(Combine(GetCurrentDirectory(), "..", "..", ".."))
                                    .Add(BudDependencies())
                                    .Set(Sources, configs => FilesObservatory[configs].ObserveFileList(Combine(ProjectDir[configs], "Build.cs")))
                                    .Get(CSharp.Compilation)
                                    .ToEnumerable()
                                    .First();
      if (compilationOutput.Success) {
        var configs = LoadBuildConf(compilationOutput);
        configs.Get<int>("hello");
      } else {
        PrintCompilationErrors(compilationOutput);
      }
      Console.ReadLine();
    }

    private static Conf LoadBuildConf(CompilationOutput compilationOutput) {
      var assembly = Assembly.LoadFile(compilationOutput.AssemblyPath);
      var buildDefinitionType = assembly.GetType("Build");
      var buildDefinition = (IBuild) buildDefinitionType.GetConstructor(Type.EmptyTypes).Invoke(new object[] {});
      return buildDefinition.Init(GetCurrentDirectory());
    }

    private static void PrintCompilationErrors(CompilationOutput compilationOutput) {
      Console.WriteLine("Could not compile the build configuration.");
      foreach (var diagnostic in compilationOutput.Diagnostics) {
        Console.WriteLine(diagnostic);
      }
    }

    private static Conf BudDependencies()
      => Conf.Empty.Set(CSharp.Dependencies, c => FilesObservatory[c].ObserveAssemblies(
        typeof(BuildTool).Assembly.Location,
        typeof(object).Assembly.Location,
        typeof(Observable).Assembly.Location));
  }
}