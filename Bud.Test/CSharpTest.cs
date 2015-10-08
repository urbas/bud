using System;
using System.Collections.Generic;
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
using static Bud.NuGet;

namespace Bud {
  public class CSharpTest {
    [Test]
    public void Invokes_the_compiler() {
      var cSharpCompiler = new Mock<ICSharpCompiler>();
      var compilationResult = Observable.Return(new Mock<ICompilationResult>().Object);

      var project = Project("fooDir", "Foo")
        .ExtendWith(CSharpCompilation())
        .Const(CSharpCompiler, cSharpCompiler.Object);

      cSharpCompiler.Setup(self => self.Compile(It.IsAny<IObservable<IFiles>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CSharpCompilationOptions>(), It.IsAny<IEnumerable<MetadataReference>>()))
                    .Returns(compilationResult);

      Assert.AreSame(compilationResult, project.Get(Compile));
    }

    [Test]
    [Ignore]
    public async void Compile_bud() {
      await Project(@"C:\Users\matej\Programming\bud\Bud")
        .ExtendWith(SourceDir(fileFilter: "*.cs"))
        .ExtendWith(ExcludeSourceDirs("obj", "bin", "target"))
        .ExtendWith(CSharpCompilation())
        .ExtendWith(NuGetPackages())
        .Get(Compile)
        .Do(result => Console.WriteLine($"\n=== COMPILING =============================\nCompiled: {result.AssemblyPath}\nSuccess: {result.EmitResult.Success} \nDiagnostics: {string.Join("\n", result.EmitResult.Diagnostics.Select(diagnostic => diagnostic.ToString()))}\n================================"))
        .ToTask();
    }

    private static Configs ExcludeSourceDirs(params string[] subDirs)
      => Configs.Empty
                .Modify(Sources, (configs, previousFiles) => {
                  var forbiddenDirs = subDirs.Select(s => Path.Combine(ProjectDir[configs], s));
                  return new FilterFiles(previousFiles, file => !forbiddenDirs.Any(file.StartsWith));
                });
  }
}