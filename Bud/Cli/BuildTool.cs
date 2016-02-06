using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Bud.Cs;
using Bud.NuGet;
using Bud.Reactive;
using Bud.Util;
using Bud.V1;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static System.IO.Directory;
using static System.IO.Path;
using static Bud.Util.Option;
using static Bud.V1.Api;
using Version = System.Version;

namespace Bud.Cli {
  public class BuildTool {
    public static void Main(string[] args) {
      var compileOutput = CompileBuildConfiguration();
      if (compileOutput.Success) {
        ExecuteCommands(args, compileOutput);
      } else {
        PrintCompilationErrors(compileOutput);
      }
    }

    public static void ExecuteCommands(IEnumerable<string> commands,
                                       CompileOutput compileOutput) {
      var buildDefinition = LoadBuildConfiguration(compileOutput.AssemblyPath);
      foreach (var command in commands) {
        ExecuteCommand(buildDefinition, command);
      }
    }

    public static Option<object> ExecuteCommand(IConf buildDefinition, string command) {
      var optionalValue = buildDefinition.TryGet<object>(command);
      if (!optionalValue.HasValue) {
        return None<object>();
      }
      var optionalResults = ObservableResults.TryCollect(optionalValue.Value);
      if (optionalResults.HasValue) {
        return optionalResults.Value as object;
      }
      var task = optionalValue.Value as Task;
      if (task != null) {
        return TaskResults.Await(task);
      }
      return optionalValue.Value;
    }

    private static CompileOutput CompileBuildConfiguration()
      => CreateBuildConfiguration().TakeOne(Compile);

    private static void PrintCompilationErrors(CompileOutput compilationOutput) {
      Console.WriteLine("Could not compile the build configuration.");
      foreach (var diagnostic in compilationOutput.Diagnostics) {
        Console.WriteLine(diagnostic);
      }
    }

    private static IConf LoadBuildConfiguration(string assemblyPath) {
      var assembly = Assembly.LoadFile(assemblyPath);
      var buildDefinitionType = assembly
        .GetExportedTypes()
        .First(typeof(IBuild).IsAssignableFrom);
      var buildDefinition = buildDefinitionType
        .GetConstructor(Type.EmptyTypes)
        .Invoke(new object[] {});
      return ((IBuild) buildDefinition).Init().ToCompiled();
    }

    private static Conf CreateBuildConfiguration()
      => CsLibrary(Combine(GetCurrentDirectory()), "BuildConf")
        .Add(AssemblyReferences, BudDependencies)
        .Clear(SourceIncludes)
        .AddSourceFile(c => Combine(ProjectDir[c], "Build.cs"));

    private static IEnumerable<string> BudDependencies { get; } = ImmutableList.Create(
      typeof(BuildTool).Assembly.Location,
      typeof(object).Assembly.Location,
      typeof(Enumerable).Assembly.Location,
      typeof(ImmutableArray).Assembly.Location,
      typeof(Observable).Assembly.Location,
      typeof(ResourceDescription).Assembly.Location,
      typeof(CSharpCompilationOptions).Assembly.Location,
      typeof(Unit).Assembly.Location,
      WindowsFrameworkAssemblyResolver.ResolveFrameworkAssembly("System.Runtime", Version.Parse("4.6.0.0")).Value,
      WindowsFrameworkAssemblyResolver.ResolveFrameworkAssembly("System.IO", Version.Parse("4.6.0.0")).Value);
  }
}