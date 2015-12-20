using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Bud.Cs;
using Bud.IO;
using Microsoft.CodeAnalysis;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using static Bud.V1.Api;

namespace Bud.V1 {
  public class ApiCSharpTest {
    [Test]
    public void Assembly_name_must_use_the_project_id() {
      using (var tempDir = new TemporaryDirectory()) {
        var cSharpProject = CsLibrary(tempDir.Path, "Foo");
        Assert.AreEqual("Foo.dll",
                        AssemblyName[cSharpProject]);
      }
    }

    [Test]
    public void CSharp_sources_must_be_listed() {
      using (var tempDir = new TemporaryDirectory()) {
        var cSharpProject = CsLibrary(tempDir.Path, "Foo");
        var sourceFile = tempDir.CreateEmptyFile("TestMainClass.cs");
        Assert.That(Sources[cSharpProject].Take(1).Wait(), Contains.Item(sourceFile));
      }
    }

    [Test]
    public void CSharp_sources_in_nested_directories_must_be_listed() {
      using (var tempDir = new TemporaryDirectory()) {
        var cSharpProject = CsLibrary(tempDir.Path, "Foo");
        var sourceFile = tempDir.CreateEmptyFile("Bud", "TestMainClass.cs");
        Assert.That(Sources[cSharpProject].Take(1).Wait(),
                    Contains.Item(sourceFile));
      }
    }

    [Test]
    public void Non_csharp_files_must_not_be_listed() {
      using (var tempDir = new TemporaryDirectory()) {
        var cSharpProject = CsLibrary(tempDir.Path, "Foo");
        var textFile = tempDir.CreateEmptyFile("Bud", "TextFile.txt");
        Assert.That(Sources[cSharpProject].Take(1).Wait(),
                    Is.Not.Contains(textFile));
      }
    }

    [Test]
    public void CompilationInput_includes_Sources_and_AssemblyReferences() {
      var projectA = CsLibrary("foo", "A")
        .SetValue(SourceIncludes, ImmutableList.Create(Watched.Watch(new[] {"A.cs"})))
        .SetValue(AssemblyReferences, ImmutableList.Create("Foo.Bar.dll"));
      var compilationInputs = Input[projectA].ToEnumerable().ToList();
      Assert.AreEqual(new[] {new InOut(InOutFile.ToInOutFile("A.cs"), Assembly.ToAssembly("Foo.Bar.dll"))},
                      compilationInputs);
    }

    [Test]
    public void CompilationInput_is_passed_to_the_compiler() {
      var inOut = new InOut(InOutFile.ToInOutFile("A.cs"), Assembly.ToAssembly("Foo.Bar.dll"));
      var cSharpCompiler = new Mock<Func<InOut, CompileOutput>>(MockBehavior.Strict);
      cSharpCompiler.Setup(self => self(It.Is<InOut>(input => inOut.Equals(input))))
                    .Returns(EmptyCompileOutput());
      var projectA = CsLibrary("foo", "A")
        .SetValue(Compiler, cSharpCompiler.Object)
        .SetValue(Input, Observable.Return(inOut));
      Compile[projectA].Wait();
      cSharpCompiler.VerifyAll();
    }

    [Test]
    public void Compiler_reinvoked_when_input_changes() {
      var testScheduler = new TestScheduler();
      var compiler = NoOpCompiler();
      var compilation = ProjectAWithUpdatingSources(testScheduler, compiler.Object)
        .Get(Output).GetEnumerator();
      testScheduler.AdvanceBy(TimeSpan.FromSeconds(5).Ticks);
      while (compilation.MoveNext()) {}
      compiler.Verify(self => self(It.IsAny<InOut>()), Times.Exactly(3));
    }

    [Test]
    public void Compiler_uses_dependencies()
      => Assert.AreEqual(new[] {new InOut(Assembly.ToAssembly("Foo.dll", true))},
                         Projects(ProjectAOutputsFooDll(42L),
                                  ProjectWithDependencies("B", "../A"))
                           .Get("B" / Input).ToEnumerable());

    private static Conf ProjectAOutputsFooDll(long initialTimestamp)
      => EmptyCSharpProject("A")
        .SetValue(Compiler, input => EmptyCompileOutput(initialTimestamp++));

    private static Conf ProjectWithDependencies(string projectId, params string[] dependencies)
      => EmptyCSharpProject(projectId)
        .SetValue(Dependencies, dependencies.ToImmutableHashSet())
        .SetValue(Compiler, input => EmptyCompileOutput(10001L));

    private static Conf EmptyCSharpProject(string projectId)
      => CsLibrary(projectId, projectId)
        .SetValue(SourceIncludes, ImmutableList.Create(Watched<string>.Empty))
        .SetValue(AssemblyReferences, ImmutableList<string>.Empty);

    private static CompileOutput EmptyCompileOutput(long timestamp = 0L)
      => new CompileOutput(Enumerable.Empty<Diagnostic>(), TimeSpan.FromMilliseconds(123), "Foo.dll", true, timestamp, null);

    private static Mock<Func<InOut, CompileOutput>> NoOpCompiler() {
      var compiler = new Mock<Func<InOut, CompileOutput>>();
      compiler.Setup(self => self(It.IsAny<InOut>())).Returns(EmptyCompileOutput());
      return compiler;
    }

    private static Watched<string> EmptyFilesWithDelayedUpdates(IScheduler testScheduler) {
      var fileUpdates = Observable.Return("foo").Delay(TimeSpan.FromSeconds(1), testScheduler)
                                  .Concat(Observable.Return("bar").Delay(TimeSpan.FromSeconds(1), testScheduler));
      return Watched.Watch(Enumerable.Empty<string>(), fileUpdates);
    }

    private static Conf ProjectAWithUpdatingSources(IScheduler testScheduler, Func<InOut, CompileOutput> compiler)
      => CsLibrary("a", "A")
        .SetValue(BuildPipelineScheduler, testScheduler)
        .SetValue(SourceIncludes, ImmutableList.Create(EmptyFilesWithDelayedUpdates(testScheduler)))
        .SetValue(AssemblyReferences, ImmutableList<string>.Empty)
        .SetValue(Compiler, compiler);
  }
}