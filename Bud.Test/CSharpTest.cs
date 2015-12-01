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
using static Bud.Cs.Assembly;
using static Bud.CSharp;
using static Bud.IO.InOutFile;

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
      var assemblies = new Files("Foo.Bar.dll");
      var files = new Files("A.cs");
      var projectA = CSharpProject("foo", "A")
        .SetValue(Sources, files)
        .SetValue(AssemblyReferences, assemblies);
      var compilationInputs = Input[projectA].ToEnumerable().ToList();
      Assert.AreEqual(new[] {new InOut(ToInOutFile("A.cs"), ToAssembly("Foo.Bar.dll"))},
                      compilationInputs);
    }

    [Test]
    public void CompilationInput_is_passed_to_the_compiler() {
      var inOut = new InOut(ToInOutFile("A.cs"), ToAssembly("Foo.Bar.dll"));
      var cSharpCompiler = new Mock<Func<InOut, CompileOutput>>(MockBehavior.Strict);
      cSharpCompiler.Setup(self => self(It.Is<InOut>(input => inOut.Equals(input))))
                    .Returns(EmptyCompileOutput());
      var projectA = CSharpProject("foo", "A")
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
      testScheduler.AdvanceBy(FromSeconds(5).Ticks);
      while (compilation.MoveNext()) {}
      compiler.Verify(self => self(It.IsAny<InOut>()), Times.Exactly(3));
    }

    [Test]
    public void Compiler_uses_dependencies()
      => Assert.AreEqual(new[] {new InOut(ToAssembly("Foo.dll", true))},
                         New(ProjectAOutputsFooDll(42L),
                             ProjectWithDependencies("B", "../A"))
                           .Get("B" / Input).ToEnumerable());

    private static Conf ProjectAOutputsFooDll(long initialTimestamp)
      => EmptyCSharpProject("A")
        .SetValue(Compiler, input => EmptyCompileOutput(initialTimestamp++));

    private static Conf ProjectWithDependencies(string projectId, params string[] dependencies)
      => EmptyCSharpProject(projectId)
        .SetValue(Dependencies, dependencies)
        .SetValue(Compiler, input => EmptyCompileOutput(10001L));

    private static Conf EmptyCSharpProject(string projectId)
      => CSharpProject(projectId, projectId)
        .SetValue(Sources, Files.Empty)
        .SetValue(AssemblyReferences, Files.Empty);

    private static CompileOutput EmptyCompileOutput(long timestamp = 0L)
      => new CompileOutput(Empty<Diagnostic>(), FromMilliseconds(123), "Foo.dll", true, timestamp, null);

    private static Mock<Func<InOut, CompileOutput>> NoOpCompiler() {
      var compiler = new Mock<Func<InOut, CompileOutput>>();
      compiler.Setup(self => self(It.IsAny<InOut>())).Returns(EmptyCompileOutput());
      return compiler;
    }

    private static Files EmptyFilesWithDelayedUpdates(IScheduler testScheduler) {
      var fileUpdates = Observable.Return("foo").Delay(FromSeconds(1), testScheduler)
                                  .Concat(Observable.Return("bar").Delay(FromSeconds(1), testScheduler));
      return new Files(Empty<string>(), fileUpdates);
    }

    private static Conf ProjectAWithUpdatingSources(IScheduler testScheduler, Func<InOut, CompileOutput> compiler)
      => CSharpProject("a", "A")
        .SetValue(BuildPipelineScheduler, testScheduler)
        .SetValue(Sources, EmptyFilesWithDelayedUpdates(testScheduler))
        .SetValue(AssemblyReferences, Files.Empty)
        .SetValue(Compiler, compiler);
  }
}