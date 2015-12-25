using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection;
using Bud.Cs;
using Bud.V1;
using Microsoft.CodeAnalysis;
using static System.IO.Directory;
using static System.IO.Path;
using static Bud.V1.Api;

namespace Bud.Cli {
  public class BuildTool {
    public static void Main(string[] args) {
      var compileOutput = CompileBuildConfiguration();
      if (compileOutput.Success) {
        var buildDefinition = LoadBuildConfiguration(compileOutput.AssemblyPath);
        foreach (var command in args) {
          buildDefinition.Get<IObservable<object>>(command)
                         .ObserveOn(new EventLoopScheduler())
                         .Wait();
        }
      } else {
        PrintCompilationErrors(compileOutput);
      }
    }

    private static CompileOutput CompileBuildConfiguration()
      => CreateBuildConfiguration().TakeOne(Compile);

    private static Conf CreateBuildConfiguration()
      => CsLibrary(Combine(GetCurrentDirectory()), "BuildConf")
        .Add(AssemblyReferences, BudDependencies)
        .Clear(SourceIncludes)
        .AddSourceFile(c => Combine(ProjectDir[c], "Build.cs"));

    private static IConf LoadBuildConfiguration(string assemblyPath) {
      var assembly = Assembly.LoadFile(assemblyPath);
      var buildDefinitionType = assembly.GetExportedTypes().First(typeof (IBuild).IsAssignableFrom);
      var buildDefinition = (IBuild) buildDefinitionType.GetConstructor(Type.EmptyTypes).Invoke(new object[] {});
      return buildDefinition.Init().ToCompiled();
    }

    private static void PrintCompilationErrors(CompileOutput compilationOutput) {
      Console.WriteLine("Could not compile the build configuration.");
      foreach (var diagnostic in compilationOutput.Diagnostics) {
        Console.WriteLine(diagnostic);
      }
    }

    private static IEnumerable<string> BudDependencies { get; } = ImmutableList.Create(
      typeof (BuildTool).Assembly.Location,
      typeof (object).Assembly.Location,
      typeof (Enumerable).Assembly.Location,
      typeof (ImmutableArray).Assembly.Location,
      typeof (Observable).Assembly.Location,
      typeof (ResourceDescription).Assembly.Location,
      "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Runtime.dll",
      "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.IO.dll");
  }
}