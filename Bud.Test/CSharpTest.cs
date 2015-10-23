using System;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Bud.Compilation;
using Bud.IO;
using NUnit.Framework;
using static Bud.Build;
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
    public void Compiles_a_csharp_file() {
      using (var tempDir = new TemporaryDirectory()) {
        var cSharpProject = CSharpProject(tempDir.Path, "Foo");
        tempDir.CreateFile("public class A {}", "A.cs");
        var compilationOutput = Compile[cSharpProject].Take(1).Wait();
        Assert.IsTrue(compilationOutput.Success);
        Assert.IsEmpty(compilationOutput.Diagnostics);
        Assert.IsTrue(File.Exists(compilationOutput.AssemblyPath));
      }
    }

    [Test]
    public void Fails_to_compile_when_the_csharp_file_contains_a_syntax_error() {
      using (var tempDir = new TemporaryDirectory()) {
        var cSharpProject = CSharpProject(tempDir.Path, "Foo");
        tempDir.CreateFile("public class", "A.cs");
        var compilationOutput = Compile[cSharpProject].Take(1).Wait();
        Assert.IsFalse(compilationOutput.Success);
        Assert.IsNotEmpty(compilationOutput.Diagnostics);
        Assert.IsFalse(File.Exists(compilationOutput.AssemblyPath));
      }
    }

    [Test]
    public void Compiler_recompiles_when_a_file_changes() {
      using (var tempDir = new TemporaryDirectory()) {
        var projectA = CSharpProject(tempDir.Path, "A");
        var compilationOutput = Compile[projectA].GetEnumerator();

        tempDir.CreateFile("public class A {}", "A.cs");
        Assert.IsTrue(compilationOutput.MoveNext());
        tempDir.CreateFile("public class A {s}", "A.cs");
        Assert.IsTrue(compilationOutput.MoveNext());
        Assert.IsFalse(compilationOutput.Current.Success);
      }
    }

    [Test]
    public void Compiler_uses_assembly_references() {
      using (var tempDir = new TemporaryDirectory()) {
        var cSharpProject = CSharpProject(tempDir.Path, "Foo")
          .Modify(AssemblyReferences, AddLinqAssembly);
        tempDir.CreateFile("using System; using System.Reactive.Linq; public class A {public IObservable<int> foo = Observable.Return(1);}", "A.cs");
        var compilationOutput = Compile[cSharpProject].Take(1).Wait();
        Assert.IsTrue(compilationOutput.Success);
      }
    }

    [Test]
    [Ignore]
    public void Compiler_uses_dependencies() {
      using (var tempDir = new TemporaryDirectory()) {
        var projectA = CSharpProject(Path.Combine(tempDir.Path, "A"), "A");
        var projectB = CSharpProject(Path.Combine(tempDir.Path, "B"), "B")
          .Const(Dependencies, new[] {"A"});

        var buildConfiguration = Projects(projectA, projectB);

        tempDir.CreateFile("public class A {}", "A", "A.cs");
        tempDir.CreateFile("public class B {public A a;}", "B", "B.cs");

        var compilationOutput = ("B" / Compile)[buildConfiguration].Take(1).Wait();
        Assert.IsTrue(compilationOutput.Success);
      }
    }

    [Test]
    [Ignore]
    public async void Compiler_recompiles_when_dependencies_change() {
      using (var tempDir = new TemporaryDirectory()) {
        tempDir.CreateFile("public class A {}", "A", "A.cs");
        tempDir.CreateFile("public class B {public A a;}", "B", "B.cs");

        var projectA = CSharpProject(Path.Combine(tempDir.Path, "A"), "A");
        var projectB = CSharpProject(Path.Combine(tempDir.Path, "B"), "B")
          .Const(Dependencies, new[] {"A"});
        var buildConfiguration = Projects(projectA, projectB);

        var compilationOutput = ("B" / Compile)[buildConfiguration]
          .Do(output => Console.WriteLine($"Compiled!!!"))
          .ToTask();

        tempDir.CreateFile("public class A {public object A;}", "A", "A.cs");

        Assert.IsTrue((await compilationOutput).Success);
      }
    }

    private static Assemblies AddLinqAssembly(IConf conf, Assemblies assemblies)
      => assemblies.ExpandWith(new Assemblies(typeof(Observable).Assembly.Location));
  }
}