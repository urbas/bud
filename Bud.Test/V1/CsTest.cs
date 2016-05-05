using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Bud.Cs;
using Bud.IO;
using Bud.NuGet;
using Microsoft.CodeAnalysis;
using Microsoft.Reactive.Testing;
using Moq;
using NuGet.Frameworks;
using NuGet.Versioning;
using NUnit.Framework;
using static Bud.V1.Api;
using static Bud.V1.Basic;
using static Bud.V1.Builds;
using static Bud.V1.Cs;

namespace Bud.V1 {
  public class CsTest {
    [Test]
    public void Assembly_name_must_use_the_project_id()
      => Assert.AreEqual("A.dll", CsLib("A", "/foo").Get(AssemblyName));

    [Test]
    public void CSharp_sources_must_be_listed() {
      using (var dir = new TemporaryDirectory()) {
        var projectA = CsLib("A", baseDir: dir.Path);
        var sourceFile = dir.CreateEmptyFile("A", "TestMainClass.cs");
        Assert.That(Sources[projectA].Take(1).Wait(),
                    Contains.Item(sourceFile));
      }
    }

    [Test]
    public void CSharp_sources_in_nested_directories_must_be_listed() {
      using (var dir = new TemporaryDirectory()) {
        var projectA = CsLib("A", baseDir: dir.Path);
        var sourceFile = dir.CreateEmptyFile("A", "B", "TestMainClass.cs");
        Assert.That(Sources[projectA].Take(1).Wait(),
                    Contains.Item(sourceFile));
      }
    }

    [Test]
    public void Non_csharp_files_must_not_be_listed() {
      using (var dir = new TemporaryDirectory()) {
        var cSharpProject = CsLib("A", baseDir: dir.Path);
        var textFile = dir.CreateEmptyFile("A", "TextFile.txt");
        Assert.That(Sources[cSharpProject].Take(1).Wait(),
                    Is.Not.Contains(textFile));
      }
    }

    [Test]
    public void Compiler_is_invoked_with_sources_and_assembly_references() {
      var cSharpCompiler = new Mock<Func<CompileInput, CompileOutput>>(MockBehavior.Strict);
      var projectA = CsLib("A", baseDir: "/foo")
        .Set(Compiler, cSharpCompiler.Object)
        .Clear(Input)
        .Add(Input, "A.cs")
        .Add(AssemblyReferences, "B.dll");
      var assemblyReferences = projectA.Get(AssemblyReferences).ToEnumerable().First();
      cSharpCompiler.Setup(self => self(new CompileInput(new[] {"A.cs"}, Enumerable.Empty<CompileOutput>(), assemblyReferences)))
                    .Returns(EmptyCompileOutput());
      Compile[projectA].Take(1).Wait();
      cSharpCompiler.VerifyAll();
    }

    [Test]
    public void Compiler_invoked_with_dependencies() {
      var cSharpCompiler = new Mock<Func<CompileInput, CompileOutput>>(MockBehavior.Strict);
      cSharpCompiler.Setup(self => self(CompileInputTestUtils.ToCompileInput(null, EmptyCompileOutput(42), null)))
                    .Returns(EmptyCompileOutput());
      var projectA = Projects(ProjectAOutputsFooDll(42),
                              ProjectWithDependencies("B", "../A")
                                .Set(Compiler, cSharpCompiler.Object));
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
    public void Mscorlib_must_be_in_the_list_of_assembly_references()
      => Assert.That(CsLib("A", baseDir: "/foo").Get(AssemblyReferences).ToEnumerable().First(),
                     Has.Some.Contains("mscorlib.dll"));

    [Test]
    public void Referenced_packages_must_be_added_to_the_list_of_assembly_references() {
      var project = CsLib("A", baseDir: "/foo")
        .Clear("Packages"/ResolvedAssemblies)
        .Add("Packages"/ResolvedAssemblies, "Bar.dll");
      Assert.That(AssemblyReferences[project].ToEnumerable().First(),
                  Has.Some.EqualTo("Bar.dll"));
    }

    [Test]
    public void Referenced_packages_project_must_reside_in_the_packages_folder()
      => Assert.AreEqual(Path.Combine("/foo", "A", "packages"),
                         CsLib("A", baseDir: "/foo").Get("Packages"/ProjectDir));

    [Test]
    public void Packages_config_file_must_be_read_from_the_ProjectDir()
      => Assert.AreEqual(Path.Combine("/foo", "A", "packages.config"),
                         CsLib("A", baseDir: "/foo").Get("Packages"/PackagesConfigFile));

    [Test]
    public void CSharp_files_in_the_packages_folder_must_not_be_listed() {
      using (var dir = new TemporaryDirectory()) {
        var csFile = dir.CreateEmptyFile("A", "packages", "A.cs");
        Assert.That(Sources[CsLib("A", baseDir: dir.Path)].Take(1).Wait(),
                    Does.Not.Contain(csFile));
      }
    }

    [Test]
    public void CSharp_files_in_the_obj_folder_must_not_be_listed() {
      using (var dir = new TemporaryDirectory()) {
        var csFile = dir.CreateEmptyFile("A", "obj", "A.cs");
        Assert.That(Sources[CsLib("A", baseDir: dir.Path)].Take(1).Wait(),
                    Does.Not.Contain(csFile));
      }
    }

    [Test]
    public void CSharp_files_in_the_bin_folder_must_not_be_listed() {
      using (var dir = new TemporaryDirectory()) {
        var csFile = dir.CreateEmptyFile("A", "bin", "A.cs");
        Assert.That(Sources[CsLib("A", baseDir: dir.Path)].Take(1).Wait(),
                    Does.Not.Contain(csFile));
      }
    }

    [Test]
    public void Package_must_contain_the_dll_of_the_CsLibrary_project() {
      var packager = new Mock<IPackager>(MockBehavior.Strict);
      var projects = Projects(CsLib("A", baseDir: "/foo")
                                .Set(ProjectVersion, "4.2.0"),
                              CsLib("B", baseDir: "/foo")
                                .Clear(Output).Add(Output, "B.dll")
                                .Set(Packager, packager.Object)
                                .Add(Dependencies, "../A"));
      packager.Setup(s => s.Pack(projects.Get("B"/PackageOutputDir),
                                 "/foo",
                                 "B",
                                 DefaultVersion,
                                 new[] {new PackageFile("B.dll", "lib/B.dll")},
                                 new[] {new PackageDependency("A", "4.2.0")},
                                 It.IsAny<NuGetPackageMetadata>()))
              .Returns("B.nupkg");
      projects.Get("B"/Package).Take(1).Wait();
      packager.VerifyAll();
    }

    [Test]
    public void Package_must_contain_references_to_own_packages() {
      var packager = new Mock<IPackager>(MockBehavior.Strict);
      var projects = Projects(CsLib("B", baseDir: "/foo")
                                .Clear(Output).Add(Output, "B.dll")
                                .Set(Packager, packager.Object)
                                .Clear("Packages"/ReferencedPackages)
                                .Add("Packages"/ReferencedPackages, new PackageReference("Foo", NuGetVersion.Parse("2.4.1"), NuGetFramework.Parse("net35"))));
      packager.Setup(s => s.Pack(projects.Get("B"/PackageOutputDir),
                                 "/foo",
                                 "B",
                                 DefaultVersion,
                                 new[] {new PackageFile("B.dll", "lib/B.dll")},
                                 new[] {new PackageDependency("Foo", "2.4.1")},
                                 It.IsAny<NuGetPackageMetadata>()))
              .Returns("B.nupkg");
      projects.Get("B"/Package).Take(1).Wait();
      packager.VerifyAll();
    }

    [Test]
    public void CsApp_produces_an_executable()
      => Assert.AreEqual("Foo.exe", CsApp("Foo").Get(AssemblyName));

    [Test]
    [Category("IntegrationTest")]
    public void Projects_must_reference_the_mscorlib_assembly() {
      using (var dir = new TemporaryDirectory()) {
        dir.CreateFile(
          "public class App {public static void Main(string[] args) => System.Console.WriteLine(\"Hello World!\");}",
          "A", "App.cs");
        var project = CsLib("A", baseDir: dir.Path);
        var compileOutput = project.Get(Compile).Take(1).Wait();
        Assert.IsTrue(compileOutput.Success);
      }
    }

    private static Conf ProjectAOutputsFooDll(long initialTimestamp)
      => EmptyCSharpProject("A")
        .Set(Compiler, input => EmptyCompileOutput(initialTimestamp++));

    private static Conf ProjectWithDependencies(string projectId,
                                                params string[] dependencies)
      => EmptyCSharpProject(projectId)
        .Add(Dependencies, dependencies);

    private static Conf EmptyCSharpProject(string projectId)
      => CsLib(projectId, projectId, "/foo")
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
      => CsLib("A", baseDir: "/foo")
        .Set(BuildPipelineScheduler, testScheduler)
        .Clear(SourceIncludes)
        .Add(SourceIncludes, FileADelayedUpdates(testScheduler))
        .Clear(AssemblyReferences)
        .Set(Compiler, compiler);
  }
}