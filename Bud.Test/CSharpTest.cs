using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Bud.Compilation;
using Bud.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Moq;
using NUnit.Framework;
using static Bud.Build;
using static Bud.CSharp;

namespace Bud {
  public class CSharpTest {
    private static readonly Configs SimpleCSharpProject = SourceDir(fileFilter: "*.cs")
      .Add(ExcludeSourceDirs("obj", "bin", "target"), CSharpCompilation());

    private static readonly Configs budProject = Project(@"../../../Bud").Add(SimpleCSharpProject, BudDependencies());
    private static readonly Configs budTestProject = Project(@"../../../Bud.Test").Add(SimpleCSharpProject, BudTestDependencies());

    [Test]
    [Ignore]
    public async void Compiles_bud() {
      var budCompilation = CSharp.Compilation[budProject]
        .Do(compilation => EmitDllAndPrintResult(compilation, budProject))
        .Select(result => result.ToMetadataReference());
      await budTestProject.Modify(References, (configs, observable) => observable.CombineLatest(budCompilation, (references, compilation) => references.Concat(new[] {compilation})))
                          .Get(CSharp.Compilation)
                          .Do(compilation => EmitDllAndPrintResult(compilation, budTestProject))
                          .ToTask();
    }

    private static void EmitDllAndPrintResult(CSharpCompilation compilation, Configs project) {
      var assemblyPath = Path.Combine(OutputDir[project], AssemblyName[project]);
      Directory.CreateDirectory(OutputDir[project]);
      using (var assemblyOutputFile = File.Create(assemblyPath)) {
        var emitResult = compilation.Emit(assemblyOutputFile);
        Console.WriteLine($"Compiled: {assemblyPath}, Success: {emitResult.Success}");
      }
    }

    private static Configs BudDependencies()
      => Configs.Empty
                .Set(References, c => Observable.Return<IEnumerable<MetadataReference>>(
                  ImmutableArray.Create(
                    MetadataReference.CreateFromFile(Path.Combine(ProjectDir[c], "../packages/Microsoft.CodeAnalysis.Common.1.1.0-beta1-20150812-01/lib/net45/Microsoft.CodeAnalysis.dll")), MetadataReference.CreateFromFile(Path.Combine(ProjectDir[c], "../packages/Microsoft.CodeAnalysis.CSharp.1.1.0-beta1-20150812-01/lib/net45/Microsoft.CodeAnalysis.CSharp.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(ProjectDir[c], "../packages/Microsoft.Web.Xdt.2.1.0/lib/net40/Microsoft.Web.XmlTransform.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(ProjectDir[c], "../packages/NuGet.Core.2.8.6/lib/net40-Client/NuGet.Core.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(ProjectDir[c], "../packages/System.Collections.Immutable.1.1.38-beta-23225/lib/dotnet/System.Collections.Immutable.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(ProjectDir[c], "../packages/Rx-Core.2.2.5/lib/net45/System.Reactive.Core.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(ProjectDir[c], "../packages/Rx-Interfaces.2.2.5/lib/net45/System.Reactive.Interfaces.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(ProjectDir[c], "../packages/Rx-Linq.2.2.5/lib/net45/System.Reactive.Linq.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(ProjectDir[c], "../packages/Rx-PlatformServices.2.2.5/lib/net45/System.Reactive.PlatformServices.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(ProjectDir[c], "../packages/System.Reflection.Metadata.1.1.0-alpha-00009/lib/dotnet/System.Reflection.Metadata.dll")),
                    MetadataReference.CreateFromFile("C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Collections.dll"),
                    MetadataReference.CreateFromFile("C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Diagnostics.Debug.dll"),
                    MetadataReference.CreateFromFile("C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Globalization.dll"),
                    MetadataReference.CreateFromFile("C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.IO.dll"),
                    MetadataReference.CreateFromFile("C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Linq.dll"),
                    MetadataReference.CreateFromFile("C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Reflection.dll"),
                    MetadataReference.CreateFromFile("C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Reflection.Extensions.dll"),
                    MetadataReference.CreateFromFile("C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Reflection.Primitives.dll"),
                    MetadataReference.CreateFromFile("C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Resources.ResourceManager.dll"),
                    MetadataReference.CreateFromFile("C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Runtime.dll"),
                    MetadataReference.CreateFromFile("C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Runtime.Extensions.dll"),
                    MetadataReference.CreateFromFile("C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Runtime.InteropServices.dll"),
                    MetadataReference.CreateFromFile("C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Text.Encoding.dll"),
                    MetadataReference.CreateFromFile("C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Text.Encoding.Extensions.dll"),
                    MetadataReference.CreateFromFile("C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Threading.dll"),
                    MetadataReference.CreateFromFile("C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Threading.Tasks.dll"),
                    MetadataReference.CreateFromFile("C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/mscorlib.dll"),
                    MetadataReference.CreateFromFile("C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/System.dll"),
                    MetadataReference.CreateFromFile("C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/System.Core.dll"))));

    private static Configs BudTestDependencies()
      => BudDependencies().Modify(References, (c, referencesObservable) => referencesObservable.Select(
        references => references.Concat(
          ImmutableArray.Create(MetadataReference.CreateFromFile(Path.Combine(ProjectDir[c], "../packages/NUnit.2.6.4/lib/nunit.framework.dll")),
                                MetadataReference.CreateFromFile(Path.Combine(ProjectDir[c], "../packages/Moq.4.2.1507.0118/lib/net40/Moq.dll"))))));

    private static Configs ExcludeSourceDirs(params string[] subDirs)
      => Configs.Empty
                .Modify(Sources, (configs, previousFiles) => {
                  var forbiddenDirs = subDirs.Select(s => Path.Combine(ProjectDir[configs], s));
                  return new FilterFiles(previousFiles, file => !forbiddenDirs.Any(file.StartsWith));
                });
  }
}