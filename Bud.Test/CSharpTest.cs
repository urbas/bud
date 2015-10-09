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
    [Test]
    public void Invokes_the_compiler() {
      var cSharpCompiler = new Mock<ICSharpCompiler>();
      var compilationResult = Observable.Return(new Mock<ICompilationResult>().Object);

      var project = Project("fooDir", "Foo")
        .Add(CSharpCompilation())
        .Const(CSharpCompiler, cSharpCompiler.Object);

      cSharpCompiler.Setup(self => self.Compile(It.IsAny<IObservable<FilesUpdate>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CSharpCompilationOptions>(), It.IsAny<IEnumerable<MetadataReference>>()))
                    .Returns(compilationResult);

      Assert.AreSame(compilationResult, project.Get(Compile));
    }

    [Test]
    [Ignore]
    public async void Compiles_bud() {
      await Project(@"../../../Bud")
        .Add(SourceDir(fileFilter: "*.cs"),
             ExcludeSourceDirs("obj", "bin", "target"),
             CSharpCompilation(),
             BudDependencies())
        .Get(Compile)
        .Do(result => Console.WriteLine($"Compilation: Success = {result.EmitResult.Success}, Time = {result.CompilationTime.Milliseconds}ms"))
        .ToTask();
    }

    [Test]
    [Ignore]
    public async void Compiles_bud_test() {
      await Project(@"../../../Bud.Test")
        .Add(SourceDir(fileFilter: "*.cs"),
             ExcludeSourceDirs("obj", "bin", "target"),
             CSharpCompilation(),
             BudTestDependencies())
        .Get(Compile)
        .Do(result => {
          Console.WriteLine($"Compilation: Success = {result.EmitResult.Success}, Time = {result.CompilationTime.Milliseconds}ms");
          if (!result.EmitResult.Success) {
            foreach (var diagnostic in result.EmitResult.Diagnostics) {
              Console.WriteLine($"{diagnostic}");
            }
          }
        })
        .ToTask();
    }

    private static Configs BudDependencies()
      => Configs.Empty
                .Set(References, c => ImmutableArray.Create(MetadataReference.CreateFromFile(Path.Combine(ProjectDir[c], "../packages/Microsoft.CodeAnalysis.Common.1.1.0-beta1-20150812-01/lib/net45/Microsoft.CodeAnalysis.dll")), MetadataReference.CreateFromFile(Path.Combine(ProjectDir[c], "../packages/Microsoft.CodeAnalysis.CSharp.1.1.0-beta1-20150812-01/lib/net45/Microsoft.CodeAnalysis.CSharp.dll")), MetadataReference.CreateFromFile(Path.Combine(ProjectDir[c], "../packages/Microsoft.Web.Xdt.2.1.0/lib/net40/Microsoft.Web.XmlTransform.dll")), MetadataReference.CreateFromFile(Path.Combine(ProjectDir[c], "../packages/NuGet.Core.2.8.6/lib/net40-Client/NuGet.Core.dll")), MetadataReference.CreateFromFile(Path.Combine(ProjectDir[c], "../packages/System.Collections.Immutable.1.1.38-beta-23225/lib/dotnet/System.Collections.Immutable.dll")), MetadataReference.CreateFromFile(Path.Combine(ProjectDir[c], "../packages/Rx-Core.2.2.5/lib/net45/System.Reactive.Core.dll")), MetadataReference.CreateFromFile(Path.Combine(ProjectDir[c], "../packages/Rx-Interfaces.2.2.5/lib/net45/System.Reactive.Interfaces.dll")), MetadataReference.CreateFromFile(Path.Combine(ProjectDir[c], "../packages/Rx-Linq.2.2.5/lib/net45/System.Reactive.Linq.dll")), MetadataReference.CreateFromFile(Path.Combine(ProjectDir[c], "../packages/Rx-PlatformServices.2.2.5/lib/net45/System.Reactive.PlatformServices.dll")), MetadataReference.CreateFromFile(Path.Combine(ProjectDir[c], "../packages/System.Reflection.Metadata.1.1.0-alpha-00009/lib/dotnet/System.Reflection.Metadata.dll")), MetadataReference.CreateFromFile("C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Collections.dll"), MetadataReference.CreateFromFile("C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Diagnostics.Debug.dll"), MetadataReference.CreateFromFile("C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Globalization.dll"), MetadataReference.CreateFromFile("C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.IO.dll"), MetadataReference.CreateFromFile("C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Linq.dll"), MetadataReference.CreateFromFile("C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Reflection.dll"), MetadataReference.CreateFromFile("C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Reflection.Extensions.dll"), MetadataReference.CreateFromFile("C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Reflection.Primitives.dll"), MetadataReference.CreateFromFile("C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Resources.ResourceManager.dll"), MetadataReference.CreateFromFile("C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Runtime.dll"), MetadataReference.CreateFromFile("C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Runtime.Extensions.dll"), MetadataReference.CreateFromFile("C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Runtime.InteropServices.dll"), MetadataReference.CreateFromFile("C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Text.Encoding.dll"), MetadataReference.CreateFromFile("C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Text.Encoding.Extensions.dll"), MetadataReference.CreateFromFile("C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Threading.dll"), MetadataReference.CreateFromFile("C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/Facades/System.Threading.Tasks.dll"), MetadataReference.CreateFromFile("C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/mscorlib.dll"), MetadataReference.CreateFromFile("C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/System.dll"), MetadataReference.CreateFromFile("C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.6/System.Core.dll")));

    private Configs BudTestDependencies()
      => BudDependencies().Modify(References, (c, references) => references.Concat(ImmutableArray.Create(MetadataReference.CreateFromFile(Path.Combine(ProjectDir[c], "../Bud/target/Bud.dll")),
                                                                                                         MetadataReference.CreateFromFile(Path.Combine(ProjectDir[c], "../packages/NUnit.2.6.4/lib/nunit.framework.dll")),
                                                                                                         MetadataReference.CreateFromFile(Path.Combine(ProjectDir[c], "../packages/Moq.4.2.1507.0118/lib/net40/Moq.dll")))));

    private static Configs ExcludeSourceDirs(params string[] subDirs)
      => Configs.Empty
                .Modify(Sources, (configs, previousFiles) => {
                  var forbiddenDirs = subDirs.Select(s => Path.Combine(ProjectDir[configs], s));
                  return new FilterFiles(previousFiles, file => !forbiddenDirs.Any(file.StartsWith));
                });
  }
}