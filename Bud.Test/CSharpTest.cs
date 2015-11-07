using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Bud.Cs;
using Bud.IO;
using Microsoft.CodeAnalysis;
using Moq;
using NUnit.Framework;
using static System.Linq.Enumerable;
using static System.Threading.Thread;
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
      var cSharpCompilationInput = EmptyCompileInput();
      var cSharpCompiler = new Mock<Func<CompileInput, CompileOutput>>(MockBehavior.Strict);
      cSharpCompiler.Setup(self => self(It.Is<CompileInput>(input => cSharpCompilationInput.Equals(input))))
                    .Returns(EmptyCompileOutput());
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
          return EmptyCompileOutput();
        })
        .Get(Compile).Wait();
      Assert.AreEqual(3, invocationCount);
    }

    [Test]
    public void Compiler_uses_dependencies() {
      var projects = Group(ProjectAWithFakeOutput(42L),
                           ProjectWithDependencies("B", "../A"));
      var compilationInput = projects.Get("B" / Compile).ToEnumerable();
      Assert.AreEqual(new[] {EmptyCompileOutput(1042)},
                      compilationInput);
    }

    [Test]
    public void Compiler_reinvoked_when_dependencies_change() {
      var projects = Group(ProjectAWithFakeOutput(9000L)
                             .SetValue(Sources, new Files(Empty<string>(), new[] {"foo"}.ToObservable())),
                           ProjectWithDependencies("B", "../A"));
      var compileOutputs = projects.Get("B" / Compile).ToEnumerable();
      Assert.AreEqual(new[] {EmptyCompileOutput(10000), EmptyCompileOutput(10001)},
                      compileOutputs);
    }

    [Test]
    public void Compilers_must_be_invoked_on_the_build_pipeline_thread() {
      int inputThreadId = 0;
      int compileThreadId = 0;
      EmptyCSharpProject("A")
        .SetValue(CompilationInput, Observable.Create<CompileInput>(observer => {
          Task.Run(() => {
            inputThreadId = CurrentThread.ManagedThreadId;
            observer.OnNext(EmptyCompileInput());
            observer.OnCompleted();
          });
          return new CompositeDisposable();
        }))
        .SetValue(Compiler, input => {
          compileThreadId = CurrentThread.ManagedThreadId;
          return EmptyCompileOutput(42);
        })
        .Get(Compile).Wait();
      Assert.AreNotEqual(inputThreadId, compileThreadId);
    }

    private static Conf ProjectAWithFakeOutput(long initialTimestamp)
      => EmptyCSharpProject("A")
        .SetValue(Compiler, input => EmptyCompileOutput(initialTimestamp++));

    private static Conf ProjectWithDependencies(string projectId, params string[] dependencies)
      => EmptyCSharpProject(projectId)
        .SetValue(Dependencies, dependencies)
        .SetValue(Compiler, MaxTimestampWithSecondDelayCompiler);

    private static CompileOutput MaxTimestampWithSecondDelayCompiler(CompileInput input)
      => EmptyCompileOutput(MaxDependencyTimestamp(input) + 1000);

    private static long MaxDependencyTimestamp(CompileInput input)
      => input.Dependencies.Any() ? input.Dependencies.Max(output => output.Timestamp) : 0L;

    private static Conf EmptyCSharpProject(string projectId)
      => CSharpProject(projectId, projectId)
        .SetValue(Sources, Files.Empty)
        .SetValue(AssemblyReferences, Assemblies.Empty);

    public static CompileInput EmptyCompileInput()
      => new CompileInput(Empty<string>(),
                          Empty<AssemblyReference>(),
                          Empty<CompileOutput>());

    private static CompileOutput EmptyCompileOutput(long timestamp = 0L)
      => new CompileOutput(Empty<Diagnostic>(), Zero, "foo", true, timestamp, null);
  }
}