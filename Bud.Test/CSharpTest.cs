using System;
using System.Collections.Immutable;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Bud.Configuration.ApiV1;
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
using static Bud.IO.Watched;

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
        Assert.That(Sources[cSharpProject].Take(1).Wait(), Contains.Item(sourceFile));
      }
    }

    [Test]
    public void CSharp_sources_in_nested_directories_must_be_listed() {
      using (var tempDir = new TemporaryDirectory()) {
        var cSharpProject = CSharpProject(tempDir.Path, "Foo");
        var sourceFile = tempDir.CreateEmptyFile("Bud", "TestMainClass.cs");
        Assert.That(Sources[cSharpProject].Take(1).Wait(),
                    Contains.Item(sourceFile));
      }
    }

    [Test]
    public void Non_csharp_files_must_not_be_listed() {
      using (var tempDir = new TemporaryDirectory()) {
        var cSharpProject = CSharpProject(tempDir.Path, "Foo");
        var textFile = tempDir.CreateEmptyFile("Bud", "TextFile.txt");
        Assert.That(Sources[cSharpProject].Take(1).Wait(),
                    Is.Not.Contains(textFile));
      }
    }

    [Test]
    public void CompilationInput_includes_Sources_and_AssemblyReferences() {
      var projectA = CSharpProject("foo", "A")
        .SetValue(SourceIncludes, ImmutableList.Create(Watch(new[] { "A.cs" })))
        .SetValue(AssemblyReferences, ImmutableList.Create("Foo.Bar.dll"));
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
                         Group(ProjectAOutputsFooDll(42L),
                               ProjectWithDependencies("B", "../A"))
                           .Get("B" / Input).ToEnumerable());

    [Test]
    public void Default_csharp_projects_have_no_package_dependencies()
      => Assert.IsEmpty(CSharpProject("A").Get(PackageDependencies));

    [Test]
    public void Projects_inherit_package_dependencies_from_their_project_dependencies()
      => Assert.AreEqual(new[] {new Package("Foo.Bar", "1.2.3", "net45")},
                         Group(CSharpProject("A")
                                 .Add(PackageDependencies, new Package("Foo.Bar", "1.2.3", "net45")),
                               CSharpProject("B")
                                 .Add(Dependencies, "../A"))
                           .Get("B" / TransitivePackageDependencies));

    [Test]
    public void No_packages_are_inheritted_from_projects_without_package_dependence_support()
      => Assert.IsEmpty(Group(Project("aDir", "A"),
                              CSharpProject("B").Add(Dependencies, "../A"))
                          .Get("B" / TransitivePackageDependencies));

    private static Conf ProjectAOutputsFooDll(long initialTimestamp)
      => EmptyCSharpProject("A")
        .SetValue(Compiler, input => EmptyCompileOutput(initialTimestamp++));

    private static Conf ProjectWithDependencies(string projectId, params string[] dependencies)
      => EmptyCSharpProject(projectId)
        .SetValue(Dependencies, dependencies.ToImmutableHashSet())
        .SetValue(Compiler, input => EmptyCompileOutput(10001L));

    private static Conf EmptyCSharpProject(string projectId)
      => CSharpProject(projectId, projectId)
        .SetValue(SourceIncludes, ImmutableList.Create(Watched<string>.Empty))
        .SetValue(AssemblyReferences, ImmutableList<string>.Empty);

    private static CompileOutput EmptyCompileOutput(long timestamp = 0L)
      => new CompileOutput(Empty<Diagnostic>(), FromMilliseconds(123), "Foo.dll", true, timestamp, null);

    private static Mock<Func<InOut, CompileOutput>> NoOpCompiler() {
      var compiler = new Mock<Func<InOut, CompileOutput>>();
      compiler.Setup(self => self(It.IsAny<InOut>())).Returns(EmptyCompileOutput());
      return compiler;
    }

    private static Watched<string> EmptyFilesWithDelayedUpdates(IScheduler testScheduler) {
      var fileUpdates = Observable.Return("foo").Delay(FromSeconds(1), testScheduler)
                                  .Concat(Observable.Return("bar").Delay(FromSeconds(1), testScheduler));
      return Watch(Empty<string>(), fileUpdates);
    }

    private static Conf ProjectAWithUpdatingSources(IScheduler testScheduler, Func<InOut, CompileOutput> compiler)
      => CSharpProject("a", "A")
        .SetValue(BuildPipelineScheduler, testScheduler)
        .SetValue(SourceIncludes, ImmutableList.Create(EmptyFilesWithDelayedUpdates(testScheduler)))
        .SetValue(AssemblyReferences, ImmutableList<string>.Empty)
        .SetValue(Compiler, compiler);
  }
}