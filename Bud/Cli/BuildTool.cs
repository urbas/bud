using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Bud.Cs;
using Bud.IO;
using Microsoft.CodeAnalysis;
using static System.IO.Directory;
using static System.IO.Path;
using static Bud.Builds;
using Assembly = System.Reflection.Assembly;

namespace Bud.Cli {
  public class BuildTool {
    public static void Main(string[] args) {
      var compilationOutput = CSharp.CSharpProject(Combine(GetCurrentDirectory()), "BuildConf")
                                    .Add(BudDependencies())
                                    .Set(SourceIncludes, configs => ImmutableList.Create(Builds.FilesObservatory[configs].ObserveFiles(Combine(ProjectDir[configs], "Build.cs"))))
                                    .Get(CSharp.Compile)
                                    .Take(1)
                                    .Wait();
      if (compilationOutput.Success) {
        var buildDefinition = LoadBuildConf(compilationOutput);
        foreach (var command in args) {
          buildDefinition.Get<IObservable<object>>(command)
                         .ObserveOn(new EventLoopScheduler())
                         .Wait();
        }
      } else {
        PrintCompilationErrors(compilationOutput);
      }
    }

    private static IConf LoadBuildConf(CompileOutput compilationOutput) {
      var assembly = Assembly.LoadFile(compilationOutput.AssemblyPath);
      var buildDefinitionType = assembly.GetExportedTypes().First(typeof(IBuild).IsAssignableFrom);
      var buildDefinition = (IBuild) buildDefinitionType.GetConstructor(Type.EmptyTypes).Invoke(new object[] {});
      return buildDefinition.Init().ToCompiled();
    }

    private static void PrintCompilationErrors(CompileOutput compilationOutput) {
      Console.WriteLine("Could not compile the build configuration.");
      foreach (var diagnostic in compilationOutput.Diagnostics) {
        Console.WriteLine(diagnostic);
      }
    }

    private static Conf BudDependencies()
      => CSharp.AssemblyReferences.Set(
        c => ImmutableList.Create(
          typeof(BuildTool).Assembly.Location,
          typeof(object).Assembly.Location,
          typeof(Enumerable).Assembly.Location,
          typeof(ImmutableArray).Assembly.Location,
          typeof(Observable).Assembly.Location,
          typeof(ResourceDescription).Assembly.Location,
          "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Runtime.dll",
          "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.IO.dll"));
  }
}