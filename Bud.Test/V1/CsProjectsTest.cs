using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Bud.Cs;
using Bud.IO;
using Microsoft.CodeAnalysis;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using static Bud.Cs.CompileInputTestUtils;
using static Bud.V1.Api;
using static NUnit.Framework.Assert;
using Contains = NUnit.Framework.Contains;

namespace Bud.V1 {
  public class CsProjectsTest {
    [Test]
    public void Assembly_name_must_use_the_project_id() {
      using (var tempDir = new TemporaryDirectory()) {
        var cSharpProject = CsLibrary(tempDir.Path, "Foo");
        AreEqual("Foo.dll",
                 AssemblyName[cSharpProject]);
      }
    }

    [Test]
    public void CSharp_sources_must_be_listed() {
      using (var tempDir = new TemporaryDirectory()) {
        var cSharpProject = CsLibrary(tempDir.Path, "Foo");
        var sourceFile = tempDir.CreateEmptyFile("TestMainClass.cs");
        That(Sources[cSharpProject].Take(1).Wait(),
             Contains.Item(sourceFile));
      }
    }

    [Test]
    public void CSharp_sources_in_nested_directories_must_be_listed() {
      using (var tempDir = new TemporaryDirectory()) {
        var cSharpProject = CsLibrary(tempDir.Path, "Foo");
        var sourceFile = tempDir.CreateEmptyFile("Bud", "TestMainClass.cs");
        That(Sources[cSharpProject].Take(1).Wait(),
             Contains.Item(sourceFile));
      }
    }

    [Test]
    public void Non_csharp_files_must_not_be_listed() {
      using (var tempDir = new TemporaryDirectory()) {
        var cSharpProject = CsLibrary(tempDir.Path, "Foo");
        var textFile = tempDir.CreateEmptyFile("Bud", "TextFile.txt");
        That(Sources[cSharpProject].Take(1).Wait(),
             Is.Not.Contains(textFile));
      }
    }

    [Test]
    public void Compiler_is_invoked_with_input_sources() {
      var cSharpCompiler = new Mock<Func<CompileInput, CompileOutput>>(MockBehavior.Strict);
      cSharpCompiler.Setup(self => self(ToCompileInput("A.cs", null, null)))
                    .Returns(EmptyCompileOutput());
      var projectA = CsLibrary("foo", "A")
        .SetValue(Compiler, cSharpCompiler.Object)
        .Clear(Input)
        .Add(Input, "A.cs");
      Compile[projectA].Take(1).Wait();
      cSharpCompiler.VerifyAll();
    }

    [Test]
    public void Compiler_is_invoked_with_assembly_references() {
      var cSharpCompiler = new Mock<Func<CompileInput, CompileOutput>>(MockBehavior.Strict);
      cSharpCompiler.Setup(self => self(ToCompileInput(null, null, "A.dll")))
                    .Returns(EmptyCompileOutput());
      var projectA = CsLibrary("foo", "A")
        .SetValue(Compiler, cSharpCompiler.Object)
        .Clear(Input)
        .Add(AssemblyReferences, "A.dll");
      Compile[projectA].Take(1).Wait();
      cSharpCompiler.VerifyAll();
    }

    [Test]
    public void Compiler_invoked_with_dependencies() {
      var cSharpCompiler = new Mock<Func<CompileInput, CompileOutput>>(MockBehavior.Strict);
      cSharpCompiler.Setup(self => self(ToCompileInput(null, EmptyCompileOutput(42), null)))
                    .Returns(EmptyCompileOutput());
      var projectA = Projects(ProjectAOutputsFooDll(42),
                              ProjectWithDependencies("B", "../A")
                                .SetValue(Compiler, cSharpCompiler.Object));
      projectA.Get("B"/Compile).Take(1).Wait();
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
      compiler.Verify(self => self(It.IsAny<CompileInput>()), Times.Exactly(3));
    }

    [Test]
    public void Referenced_packages_must_be_added_to_the_list_of_assembly_references() {
      var project = CsLibrary("Foo")
        .Clear("NuGetPackageReference"/ResolvedAssemblies)
        .Add("NuGetPackageReference"/ResolvedAssemblies, "Bar.dll");
      That(AssemblyReferences[project].ToEnumerable(),
           Has.Exactly(1).EqualTo(new[] {"Bar.dll"}));
    }

    [Test]
    public void Referenced_packages_project_must_reside_in_the_packages_folder()
      => AreEqual(Path.Combine("Foo", "packages"),
                  CsLibrary("Foo").Get("NuGetPackageReference"/ProjectDir));

    [Test]
    public void Packages_config_file_must_be_read_from_the_root()
      => AreEqual(Path.Combine("Foo", "packages.config"),
                  CsLibrary("Foo").Get("NuGetPackageReference"/PackagesConfigFile));

    [Test]
    public void CSharp_files_in_the_packages_folder_must_not_be_listed() {
      using (var tempDir = new TemporaryDirectory()) {
        var csFile = tempDir.CreateEmptyFile("packages", "A.cs");
        That(Sources[CsLibrary(tempDir.Path, "Foo")].Take(1).Wait(),
             Is.Not.Contains(csFile));
      }
    }

    [Test]
    public void CSharp_files_in_the_obj_folder_must_not_be_listed() {
      using (var tempDir = new TemporaryDirectory()) {
        var csFile = tempDir.CreateEmptyFile("obj", "A.cs");
        That(Sources[CsLibrary(tempDir.Path, "Foo")].Take(1).Wait(),
             Is.Not.Contains(csFile));
      }
    }

    [Test]
    public void CSharp_files_in_the_bin_folder_must_not_be_listed() {
      using (var tempDir = new TemporaryDirectory()) {
        var csFile = tempDir.CreateEmptyFile("bin", "A.cs");
        That(Sources[CsLibrary(tempDir.Path, "Foo")].Take(1).Wait(),
             Is.Not.Contains(csFile));
      }
    }

    [Test]
    public void CSharp_files_in_the_target_folder_must_not_be_listed() {
      using (var tempDir = new TemporaryDirectory()) {
        var csFile = tempDir.CreateEmptyFile("target", "A.cs");
        That(Sources[CsLibrary(tempDir.Path, "Foo")].Take(1).Wait(),
             Is.Not.Contains(csFile));
      }
    }

    private static Conf ProjectAOutputsFooDll(long initialTimestamp)
      => EmptyCSharpProject("A")
        .SetValue(Compiler, input => EmptyCompileOutput(initialTimestamp++));

    private static Conf ProjectWithDependencies(string projectId,
                                                params string[] dependencies)
      => EmptyCSharpProject(projectId)
        .Add(Dependencies, dependencies);

    private static Conf EmptyCSharpProject(string projectId)
      => CsLibrary(projectId, projectId)
        .Clear(SourceIncludes)
        .Clear(AssemblyReferences);

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
      => CsLibrary("a", "A")
        .SetValue(BuildPipelineScheduler, testScheduler)
        .Clear(SourceIncludes)
        .Add(SourceIncludes, FileADelayedUpdates(testScheduler))
        .Clear(AssemblyReferences)
        .SetValue(Compiler, compiler);
  }
}