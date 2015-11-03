using System;
using System.Reactive.Linq;
using Bud.Cs;
using Bud.IO;
using Microsoft.CodeAnalysis;
using Moq;
using NUnit.Framework;
using static System.Linq.Enumerable;
using static System.TimeSpan;
using static Bud.Build;
using static Bud.Conf;
using static Bud.CSharp;

namespace Bud {
  public class CSharpTest {
    [Test]
    public void Assembly_name_must_use_the_project_id() {
      using (var tempDir = new TemporaryDirectory()) {
        var cSharpProject = CSharpProject(tempDir.Path, "Foo");
        Assert.AreEqual("Foo.dll",
                        AssemblyName[cSharpProject]);
      }
    }

    [Test]
    public void CSharp_sources_must_be_listed() {
      using (var tempDir = new TemporaryDirectory()) {
        var cSharpProject = CSharpProject(tempDir.Path, "Foo");
        var sourceFile = tempDir.CreateEmptyFile("TestMainClass.cs");
        Assert.That(Sources[cSharpProject],
                    Contains.Item(sourceFile));
      }
    }

    [Test]
    public void CSharp_sources_in_nested_directories_must_be_listed() {
      using (var tempDir = new TemporaryDirectory()) {
        var cSharpProject = CSharpProject(tempDir.Path, "Foo");
        var sourceFile = tempDir.CreateEmptyFile("Bud", "TestMainClass.cs");
        Assert.That(Sources[cSharpProject],
                    Contains.Item(sourceFile));
      }
    }

    [Test]
    public void Non_csharp_files_must_not_be_listed() {
      using (var tempDir = new TemporaryDirectory()) {
        var cSharpProject = CSharpProject(tempDir.Path, "Foo");
        var textFile = tempDir.CreateEmptyFile("Bud", "TextFile.txt");
        Assert.That(Sources[cSharpProject],
                    Is.Not.Contains(textFile));
      }
    }

    [Test]
    public void CompilationInput_includes_Sources_and_AssemblyReferences() {
      var assemblies = new Assemblies(new[] {new AssemblyReference("Foo.Bar.dll", null)});
      var files = new Files(new[] {"A.cs"});
      var projectA = CSharpProject("foo", "A")
        .SetValue(Sources, files)
        .SetValue(AssemblyReferences, assemblies);
      var compilationInputs = CompilationInput[projectA].ToEnumerable().ToList();
      Assert.AreEqual(new[] {new CompileInput(files, assemblies, Empty<CompileOutput>())},
                      compilationInputs);
    }

    [Test]
    public void CompilationInput_is_passed_to_the_compiler() {
      var cSharpCompilationInput = new CompileInput(Files.Empty, Assemblies.Empty, Empty<CompileOutput>());
      var cSharpCompiler = new Mock<Func<CompileInput, CompileOutput>>(MockBehavior.Strict);
      cSharpCompiler.Setup(self => self(It.Is<CompileInput>(input => cSharpCompilationInput.Equals(input))))
                    .Returns(GetEmptyCompileOutput());
      var projectA = CSharpProject("foo", "A")
        .SetValue(Compiler, cSharpCompiler.Object)
        .SetValue(CompilationInput, Observable.Return(cSharpCompilationInput));
      Compile[projectA].Wait();
      cSharpCompiler.VerifyAll();
    }

    [Test]
    public void Compiler_reinvoked_when_input_changes() {
      int invocationCount = 0;
      CSharpProject("foo", "A")
        .SetValue(Sources, new Files(Empty<string>(), new[] {"foo", "bar"}.ToObservable()))
        .SetValue(AssemblyReferences, Assemblies.Empty)
        .SetValue(Compiler, input => {
          ++invocationCount;
          return GetEmptyCompileOutput();
        })
        .Get(Compile).Wait();
      Assert.AreEqual(3, invocationCount);
    }

    [Test]
    public void Compiler_uses_dependencies() {
      var projects = Group(ProjectAWithFakeOutput(42L),
                           ProjectBDependingOnA());
      var compilationInput = projects.Get("B" / Compile).ToEnumerable();
      Assert.AreEqual(new[] {GetEmptyCompileOutput(1042)},
                      compilationInput);
    }

    [Test]
    public void Compiler_reinvoked_when_dependencies_change() {
      var projects = Group(ProjectAWithFakeOutput(9000L)
                             .SetValue(Sources, new Files(Empty<string>(), new[] {"foo"}.ToObservable())),
                           ProjectBDependingOnA());
      var compileOutputs = projects.Get("B" / Compile).ToEnumerable();
      Assert.AreEqual(new[] {GetEmptyCompileOutput(10000), GetEmptyCompileOutput(10001)},
                      compileOutputs);
    }

    private static Conf ProjectAWithFakeOutput(long initialTimestamp)
      => EmptyCSharpProject("A")
        .SetValue(Compiler, input => GetEmptyCompileOutput(initialTimestamp++));

    private Conf ProjectBDependingOnA()
      => EmptyCSharpProject("B")
        .SetValue(Dependencies, new[] {"../A"})
        .SetValue(Compiler, SingleDependencyCompiler);

    private CompileOutput SingleDependencyCompiler(CompileInput input)
      => GetEmptyCompileOutput(input.Dependencies.Single().Timestamp + 1000);

    private static Conf EmptyCSharpProject(string projectId)
      => CSharpProject(projectId, projectId)
        .SetValue(Sources, Files.Empty)
        .SetValue(AssemblyReferences, Assemblies.Empty);

    private static CompileOutput GetEmptyCompileOutput(long timestamp = 0L)
      => new CompileOutput(Empty<Diagnostic>(), Zero, "foo", true, timestamp, null);
  }
}