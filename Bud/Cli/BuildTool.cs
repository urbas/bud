using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using Bud.Compilation;
using Bud.IO;
using static System.IO.Directory;
using static System.IO.Path;
using static Bud.Build;

namespace Bud.Cli {
  public class BuildTool {
    public static void Main(string[] args) {
      var compilationOutput = CSharp.CSharpProject(Combine(GetCurrentDirectory()), "BuildConf")
                                    .Add(BudDependencies())
                                    .Set(Sources, configs => Build.FilesObservatory[configs].ObserveFiles(Combine(ProjectDir[configs], "Build.cs")))
                                    .Get(CSharp.Compile)
                                    .ToEnumerable()
                                    .First();
      if (compilationOutput.Success) {
        var configs = LoadBuildConf(compilationOutput);
        foreach (var s in args) {
          configs.Get<IObservable<object>>(s).Wait();
        }
      } else {
        PrintCompilationErrors(compilationOutput);
      }
    }

    private static IConf LoadBuildConf(CSharpCompilationOutput compilationOutput) {
      var assembly = Assembly.LoadFile(compilationOutput.AssemblyPath);
      var buildDefinitionType = assembly.GetExportedTypes().First(typeof(IBuild).IsAssignableFrom);
      var buildDefinition = (IBuild) buildDefinitionType.GetConstructor(Type.EmptyTypes).Invoke(new object[] {});
      return buildDefinition.Init(GetCurrentDirectory()).ToCachingConfigs();
    }

    private static void PrintCompilationErrors(CSharpCompilationOutput compilationOutput) {
      Console.WriteLine("Could not compile the build configuration.");
      foreach (var diagnostic in compilationOutput.Diagnostics) {
        Console.WriteLine(diagnostic);
      }
    }

    private static Conf BudDependencies()
      => Conf.Empty.Set(CSharp.AssemblyReferences,
                        c => new Assemblies(typeof(BuildTool).Assembly.Location,
                                            typeof(object).Assembly.Location,
                                            typeof(Enumerable).Assembly.Location,
                                            typeof(Observable).Assembly.Location));
  }
}