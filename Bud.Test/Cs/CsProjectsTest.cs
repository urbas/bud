using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Bud.IO;
using Bud.V1;
using Microsoft.CodeAnalysis;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using Contains = NUnit.Framework.Contains;

namespace Bud.Cs {
  public class CsProjectsTest {
    [Test]
    public void Assembly_name_must_use_the_project_id() {
      using (var tempDir = new TemporaryDirectory()) {
        var cSharpProject = Api.CsLibrary(tempDir.Path, "Foo");
        Assert.AreEqual("Foo.dll",
                 Api.AssemblyName[cSharpProject]);
      }
    }

    [Test]
    public void CSharp_sources_must_be_listed() {
      using (var tempDir = new TemporaryDirectory()) {
        var cSharpProject = Api.CsLibrary(tempDir.Path, "Foo");
        var sourceFile = tempDir.CreateEmptyFile("TestMainClass.cs");
        Assert.That(Api.Sources[cSharpProject].Take(1).Wait(),
             Contains.Item(sourceFile));
      }
    }

    [Test]
    public void CSharp_sources_in_nested_directories_must_be_listed() {
      using (var tempDir = new TemporaryDirectory()) {
        var cSharpProject = Api.CsLibrary(tempDir.Path, "Foo");
        var sourceFile = tempDir.CreateEmptyFile("Bud", "TestMainClass.cs");
        Assert.That(Api.Sources[cSharpProject].Take(1).Wait(),
             Contains.Item(sourceFile));
      }
    }

    [Test]
    public void Non_csharp_files_must_not_be_listed() {
      using (var tempDir = new TemporaryDirectory()) {
        var cSharpProject = Api.CsLibrary(tempDir.Path, "Foo");
        var textFile = tempDir.CreateEmptyFile("Bud", "TextFile.txt");
        Assert.That(Api.Sources[cSharpProject].Take(1).Wait(),
             Is.Not.Contains(textFile));
      }
    }

    [Test]
    public void Compiler_is_invoked_with_input_sources() {
      var cSharpCompiler = new Mock<Func<CompileInput, CompileOutput>>(MockBehavior.Strict);
      cSharpCompiler.Setup(self => self(CompileInputTestUtils.ToCompileInput("A.cs", null, null)))
                    .Returns(EmptyCompileOutput());
      var projectA = Api.CsLibrary("foo", "A")
        .SetValue(Api.Compiler, cSharpCompiler.Object)
        .Clear(Api.Input)
        .Add(Api.Input, "A.cs");
      Api.Compile[projectA].Take(1).Wait();
      cSharpCompiler.VerifyAll();
    }

    [Test]
    public void Compiler_is_invoked_with_assembly_references() {
      var cSharpCompiler = new Mock<Func<CompileInput, CompileOutput>>(MockBehavior.Strict);
      cSharpCompiler.Setup(self => self(CompileInputTestUtils.ToCompileInput(null, null, "A.dll")))
                    .Returns(EmptyCompileOutput());
      var projectA = Api.CsLibrary("foo", "A")
        .SetValue(Api.Compiler, cSharpCompiler.Object)
        .Clear(Api.Input)
        .Add(Api.AssemblyReferences, "A.dll");
      Api.Compile[projectA].Take(1).Wait();
      cSharpCompiler.VerifyAll();
    }

    [Test]
    public void Compiler_invoked_with_dependencies() {
      var cSharpCompiler = new Mock<Func<CompileInput, CompileOutput>>(MockBehavior.Strict);
      cSharpCompiler.Setup(self => self(CompileInputTestUtils.ToCompileInput(null, EmptyCompileOutput(42), null)))
                    .Returns(EmptyCompileOutput());
      var projectA = Api.Projects(ProjectAOutputsFooDll(42),
                              ProjectWithDependencies("B", "../A")
                                .SetValue(Api.Compiler, cSharpCompiler.Object));
      projectA.Get("B"/Api.Compile).Take(1).Wait();
      cSharpCompiler.VerifyAll();
    }

    [Test]
    public void Compiler_reinvoked_when_input_changes() {
      var testScheduler = new TestScheduler();
      var compiler = NoOpCompiler();
      var compilation = ProjectAWithUpdatingSources(testScheduler, compiler.Object)
        .Get(Api.Output).GetEnumerator();
      testScheduler.AdvanceBy(TimeSpan.FromSeconds(5).Ticks);
      while (compilation.MoveNext()) {}
      compiler.Verify(self => self(It.IsAny<CompileInput>()), Times.Exactly(3));
    }

    [Test]
    public void Referenced_packages_must_be_added_to_the_list_of_assembly_references() {
      var project = Api.CsLibrary("Foo")
        .Clear("NuGetPackageReference"/Api.ResolvedAssemblies)
        .Add("NuGetPackageReference"/Api.ResolvedAssemblies, "Bar.dll");
      Assert.That(Api.AssemblyReferences[project].ToEnumerable(),
           Has.Exactly(1).EqualTo(new[] {"Bar.dll"}));
    }

    [Test]
    public void Referenced_packages_project_must_reside_in_the_packages_folder()
      => Assert.AreEqual(Path.Combine("Foo", "packages"),
                  Api.CsLibrary("Foo").Get("NuGetPackageReference"/Api.ProjectDir));

    [Test]
    public void Packages_config_file_must_be_read_from_the_root()
      => Assert.AreEqual(Path.Combine("Foo", "packages.config"),
                  Api.CsLibrary("Foo").Get("NuGetPackageReference"/Api.PackagesConfigFile));

    [Test]
    public void CSharp_files_in_the_packages_folder_must_not_be_listed() {
      using (var tempDir = new TemporaryDirectory()) {
        var csFile = tempDir.CreateEmptyFile("packages", "A.cs");
        Assert.That(Api.Sources[Api.CsLibrary(tempDir.Path, "Foo")].Take(1).Wait(),
             Is.Not.Contains(csFile));
      }
    }

    [Test]
    public void CSharp_files_in_the_obj_folder_must_not_be_listed() {
      using (var tempDir = new TemporaryDirectory()) {
        var csFile = tempDir.CreateEmptyFile("obj", "A.cs");
        Assert.That(Api.Sources[Api.CsLibrary(tempDir.Path, "Foo")].Take(1).Wait(),
             Is.Not.Contains(csFile));
      }
    }

    [Test]
    public void CSharp_files_in_the_bin_folder_must_not_be_listed() {
      using (var tempDir = new TemporaryDirectory()) {
        var csFile = tempDir.CreateEmptyFile("bin", "A.cs");
        Assert.That(Api.Sources[Api.CsLibrary(tempDir.Path, "Foo")].Take(1).Wait(),
             Is.Not.Contains(csFile));
      }
    }

    [Test]
    public void CSharp_files_in_the_target_folder_must_not_be_listed() {
      using (var tempDir = new TemporaryDirectory()) {
        var csFile = tempDir.CreateEmptyFile("target", "A.cs");
        Assert.That(Api.Sources[Api.CsLibrary(tempDir.Path, "Foo")].Take(1).Wait(),
             Is.Not.Contains(csFile));
      }
    }

    private static Conf ProjectAOutputsFooDll(long initialTimestamp)
      => EmptyCSharpProject("A")
        .SetValue(Api.Compiler, input => EmptyCompileOutput(initialTimestamp++));

    private static Conf ProjectWithDependencies(string projectId,
                                                params string[] dependencies)
      => EmptyCSharpProject(projectId)
        .Add(Api.Dependencies, dependencies);

    private static Conf EmptyCSharpProject(string projectId)
      => Api.CsLibrary(projectId, projectId)
        .Clear(Api.SourceIncludes)
        .Clear(Api.AssemblyReferences);

    private static CompileOutput EmptyCompileOutput(long timestamp = 0L)
      => new CompileOutput(Enumerable.Empty<Diagnostic>(),
                           TimeSpan.FromMilliseconds(123),
                           "Foo.dll",
                           true,
                           timestamp,
                           null);

    private static Mock<Func<CompileInput, CompileOutput>> NoOpCompiler() {
      var compiler = new Mock<Func<CompileInput, CompileOutput>>();
      compiler.Setup(self => self(It.IsAny<CompileInput>()))
              .Returns(EmptyCompileOutput());
      return compiler;
    }

    private static FileWatcher FileADelayedUpdates(IScheduler testScheduler) {
      var changes = Observable.Return("A.cs").Delay(TimeSpan.FromSeconds(1),
                                                    testScheduler)
                              .Concat(Observable.Return("A.cs")
                                                .Delay(TimeSpan.FromSeconds(1),
                                                       testScheduler));
      IEnumerable<string> files = new[] {"A.cs"};
      return new FileWatcher(files, changes);
    }

    private static Conf
      ProjectAWithUpdatingSources(IScheduler testScheduler,
                                  Func<CompileInput, CompileOutput> compiler)
      => Api.CsLibrary("a", "A")
        .SetValue(Api.BuildPipelineScheduler, testScheduler)
        .Clear(Api.SourceIncludes)
        .Add(Api.SourceIncludes, FileADelayedUpdates(testScheduler))
        .Clear(Api.AssemblyReferences)
        .SetValue(Api.Compiler, compiler);
  }
}