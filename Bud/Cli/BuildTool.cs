using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Bud.Compilation;
using static System.IO.Directory;
using static System.IO.Path;
using static Bud.Build;
using static Bud.IO.FilesObservatory;

namespace Bud.Cli {
  public class BuildTool {
    public static void Main(string[] args) {
      var compilationOutput = CSharp.CSharpProject(Combine(GetCurrentDirectory(), "..", "..", ".."), "BuildConf")
                                    .Add(BudDependencies())
                                    .Set(Sources, configs => FilesObservatory[configs].ObserveFileList(Combine(ProjectDir[configs], "Build.cs")))
                                    .Get(CSharp.Compilation)
                                    .ToEnumerable()
                                    .First();
      if (compilationOutput.Success) {
        var configs = LoadBuildConf(compilationOutput);
        foreach (var s in args) {
          configs.Get<Task>(s).Wait();
        }
      } else {
        PrintCompilationErrors(compilationOutput);
      }
    }

    private static IConf LoadBuildConf(CompilationOutput compilationOutput) {
      var assembly = Assembly.LoadFile(compilationOutput.AssemblyPath);
      var buildDefinitionType = assembly.GetType("Build");
      var buildDefinition = (IBuild) buildDefinitionType.GetConstructor(Type.EmptyTypes).Invoke(new object[] {});
      return buildDefinition.Init(GetCurrentDirectory()).ToCachingConfigs();
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