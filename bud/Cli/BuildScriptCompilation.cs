using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Bud.Cs;
using Bud.NuGet;
using Bud.V1;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json;
using static Bud.V1.Api;

namespace Bud.Cli {
  public static class BuildScriptCompilation {
    private static IEnumerable<string> BudDependencies { get; } = ImmutableList.Create(
      typeof(BuildTool).Assembly.Location,
      typeof(object).Assembly.Location,
      typeof(Enumerable).Assembly.Location,
      typeof(ImmutableArray).Assembly.Location,
      typeof(Observable).Assembly.Location,
      typeof(ResourceDescription).Assembly.Location,
      typeof(CSharpCompilationOptions).Assembly.Location,
      typeof(Unit).Assembly.Location,
      typeof(ZipArchive).Assembly.Location,
      typeof(CompressionLevel).Assembly.Location,
      typeof(JsonSerializer).Assembly.Location,
      WindowsFrameworkAssemblyResolver.ResolveFrameworkAssembly("System.Net.Http", Version.Parse("4.6.0.0")).Value,
      WindowsFrameworkAssemblyResolver.ResolveFrameworkAssembly("System.Runtime", Version.Parse("4.6.0.0")).Value,
      WindowsFrameworkAssemblyResolver.ResolveFrameworkAssembly("System.IO", Version.Parse("4.6.0.0")).Value);

    public static CompileOutput CompileBuildScript(string baseDir, string buildScriptPath)
      => BuildScriptCompiler
        .Set(BaseDir, baseDir)
        .TakeOne(Compile);

    private static Conf BuildScriptCompiler { get; }
      = CsLib("build")
        .Add(AssemblyReferences, BudDependencies)
        .Clear(SourceIncludes)
        .AddSourceFile(c => Path.Combine(BaseDir[c], "Build.cs"));
  }
}