using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Bud.Cs;
using Bud.IO;
using Microsoft.CodeAnalysis;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using static System.Linq.Enumerable;
using static System.TimeSpan;
using static Bud.Builds;
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
        Assert.That(Sources[cSharpProject].Lister, Contains.Item(sourceFile));
      }
    }

    [Test]
    public void CSharp_sources_in_nested_directories_must_be_listed() {
      using (var tempDir = new TemporaryDirectory()) {
        var cSharpProject = CSharpProject(tempDir.Path, "Foo");
        var sourceFile = tempDir.CreateEmptyFile("Bud", "TestMainClass.cs");
        Assert.That(Sources[cSharpProject].Lister,
                    Contains.Item(sourceFile));
      }
    }

    [Test]
    public void Non_csharp_files_must_not_be_listed() {
      using (var tempDir = new TemporaryDirectory()) {
        var cSharpProject = CSharpProject(tempDir.Path, "Foo");
        var textFile = tempDir.CreateEmptyFile("Bud", "TextFile.txt");
        Assert.That(Sources[cSharpProject].Lister,
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
      Assert.AreEqual(new[] {new CompileInput(files.Lister, assemblies.Lister, Empty<CompileOutput>())},
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
      var testScheduler = new TestScheduler();
      var compiler = NoOpCompiler();
      var compilation = ProjectAWithUpdatingSources(testScheduler, compiler.Object)
        .Get(Compile).GetEnumerator();
      testScheduler.AdvanceBy(FromSeconds(5).Ticks);
      while (compilation.MoveNext()) {}
      compiler.Verify(self => self(It.IsAny<CompileInput>()), Times.Exactly(3));
    }

    [Test]
    public void Compiler_uses_dependencies() {
      var projects = New(ProjectAWithFakeOutput(42L),
                         ProjectWithDependencies("B", "../A"));
      var compilationInput = projects.Get("B" / Compile).ToEnumerable();
      Assert.AreEqual(new[] {EmptyCompileOutput(1042)},
                      compilationInput);
    }

    [Test]
    public void Compiler_reinvoked_when_dependencies_change() {
      var testScheduler = new TestScheduler();
      var compilerMock = NoOpCompiler();
      var compilation = New(ProjectAWithUpdatingSources(testScheduler, _ => EmptyCompileOutput()),
                            ProjectWithDependencies("B", "../A")
                              .SetValue(BuildPipelineScheduler, testScheduler)
                              .SetValue(Compiler, compilerMock.Object))
        .Get("B" / Compile).GetEnumerator();
      testScheduler.AdvanceBy(FromSeconds(5).Ticks);
      while (compilation.MoveNext()) {}
      compilerMock.Verify(self => self(It.IsAny<CompileInput>()), Times.Exactly(3));
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

    private static Mock<Func<CompileInput, CompileOutput>> NoOpCompiler() {
      var compiler = new Mock<Func<CompileInput, CompileOutput>>();
      compiler.Setup(self => self(It.IsAny<CompileInput>())).Returns(EmptyCompileOutput());
      return compiler;
    }

    private static Files EmptyFilesWithDelayedUpdates(IScheduler testScheduler) {
      var fileUpdates = Observable.Return("foo").Delay(FromSeconds(1), testScheduler)
                                  .Concat(Observable.Return("bar").Delay(FromSeconds(1), testScheduler));
      return new Files(Empty<string>(), fileUpdates);
    }

    private static Conf ProjectAWithUpdatingSources(IScheduler testScheduler, Func<CompileInput, CompileOutput> compiler)
      => CSharpProject("a", "A")
        .SetValue(BuildPipelineScheduler, testScheduler)
        .SetValue(Sources, EmptyFilesWithDelayedUpdates(testScheduler))
        .SetValue(AssemblyReferences, Assemblies.Empty)
        .SetValue(Compiler, compiler);
  }
}