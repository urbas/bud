using System;
using System.IO;
using Bud.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using static System.Linq.Enumerable;

namespace Bud.Cs {
  [Category("IntegrationTest")]
  public class RoslynCsCompilationTest {
    [Test]
    public void Fails_to_compile_when_core_assembly_references_are_missing() {
      using (var tempDir = new TemporaryDirectory()) {
        var compiler = CreateCompiler();
        var fileA = tempDir.CreateFile("public class A {}", "A.cs");
        var compilationOutput = compiler.Compile(new[] {fileA}, Empty<string>());
        var outputPath = Path.Combine(tempDir.Path, "A.dll");
        var emitResult = compilationOutput.Emit(outputPath);
        Assert.IsFalse(emitResult.Success);
        Assert.IsNotEmpty(emitResult.Diagnostics);
        Assert.IsTrue(File.Exists(outputPath));
      }
    }

    [Test]
    public void Compiles_a_csharp_file() {
      using (var tempDir = new TemporaryDirectory()) {
        var compiler = CreateCompiler();
        var fileA = tempDir.CreateFile("public class A {}", "A.cs");
        var compilationOutput = compiler.Compile(new[] {fileA}, new[] {typeof(object).Assembly.Location});
        var outputPath = Path.Combine(tempDir.Path, "A.dll");
        var emitResult = compilationOutput.Emit(outputPath);
        Assert.IsTrue(emitResult.Success);
        Assert.IsEmpty(emitResult.Diagnostics);
        Assert.IsTrue(File.Exists(outputPath));
      }
    }

    [Test]
    public void Fails_to_compile_when_the_csharp_file_contains_a_syntax_error() {
      using (var tempDir = new TemporaryDirectory()) {
        var compiler = CreateCompiler();
        var fileA = tempDir.CreateFile("public class", "A.cs");
        var compilationOutput = compiler.Compile(new[] {fileA}, new[] {typeof(object).Assembly.Location});
        var outputPath = Path.Combine(tempDir.Path, "A.dll");
        var emitResult = compilationOutput.Emit(outputPath);
        Assert.IsFalse(emitResult.Success);
        Assert.IsNotEmpty(emitResult.Diagnostics);
        Assert.IsTrue(File.Exists(outputPath));
      }
    }

    [Test]
    public void Recompiles_when_source_files_change() {
      using (var tempDir = new TemporaryDirectory()) {
        var compiler = CreateCompiler();
        var fileA = tempDir.CreateFile("public class", "A.cs");
        var outputPath = Path.Combine(tempDir.Path, "A.dll");
        Assert.IsFalse(compiler.Compile(new[] {fileA}, new[] {typeof(object).Assembly.Location}).Emit(outputPath).Success);
        File.WriteAllText(fileA, "public class A {}");
        Assert.IsTrue(compiler.Compile(new[] {fileA}, new[] {typeof(object).Assembly.Location}).Emit(outputPath).Success);
      }
    }

    [Test]
    public void Recompiles_when_assembly_references_change() {
      using (var tempDir = new TemporaryDirectory()) {
        var compiler = CreateCompiler();
        var fileA = tempDir.CreateFile("public class A {}", "A.cs");
        var outputPath = Path.Combine(tempDir.Path, "A.dll");
        Assert.IsFalse(compiler.Compile(new[] {fileA}, Empty<string>()).Emit(outputPath).Success);
        Assert.IsTrue(compiler.Compile(new[] {fileA}, new[] {typeof(object).Assembly.Location}).Emit(outputPath).Success);
      }
    }

    [Test]
    public void Fails_to_compile_when_dependency_is_missing() {
      using (var tempDir = new TemporaryDirectory()) {
        var compiler = CreateCompiler();
        var fileB = tempDir.CreateFile("public class B : A {}", "B.cs");
        var compilationOutput = compiler.Compile(new[] {fileB}, new[] {typeof(object).Assembly.Location});
        var outputPath = Path.Combine(tempDir.Path, "B.dll");
        Assert.IsFalse(compilationOutput.Emit(outputPath).Success);
      }
    }

    [Test]
    public void Compiles_when_dependency_is_present() {
      using (var tempDir = new TemporaryDirectory()) {
        var compiler = CreateCompiler();
        var fileA = tempDir.CreateFile("public class A {}", "A.cs");
        var fileB = tempDir.CreateFile("public class B : A {}", "B.cs");
        var aOutputAssembly = Path.Combine(tempDir.Path, "A.dll");
        compiler.Compile(new[] {fileA}, new[] {typeof(object).Assembly.Location}).Emit(aOutputAssembly);
        var b = compiler.Compile(new[] {fileB}, new[] {typeof(object).Assembly.Location, aOutputAssembly});
        var outputPath = Path.Combine(tempDir.Path, "B.dll");
        var emitResult = b.Emit(outputPath);
        Assert.IsTrue(emitResult.Success);
      }
    }

    [Test]
    public void Compiles_when_internals_of_dependency_are_used() {
      using (var tempDir = new TemporaryDirectory()) {
        var fileA = tempDir.CreateFile("public class A {internal const int I=42;}", "A.cs");
        var fileAssemblyInfo = tempDir.CreateFile("public class A {internal const int I=42;}", "A.cs");
        var fileB = tempDir.CreateFile("using System.Runtime.CompilerServices;\n" +
                                       "[assembly: InternalsVisibleTo(\"B\")]", "AssemblyInfo.cs");
        var assemblyA = Path.Combine(tempDir.Path, "A.dll");
        new RoslynCsCompilation("A", new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
          .Compile(new[] {fileA, fileAssemblyInfo}, new[] {typeof(object).Assembly.Location})
          .Emit(assemblyA);
        var emitResult = new RoslynCsCompilation("B", new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
          .Compile(new[] {fileB}, new[] {typeof(object).Assembly.Location, assemblyA})
          .Emit(Path.Combine(tempDir.Path, "B.dll"));
        Console.WriteLine($"{string.Join("\n", emitResult.Diagnostics)}");
        Assert.IsTrue(emitResult.Success);
      }
    }


    private static RoslynCsCompilation CreateCompiler()
      => new RoslynCsCompilation("A.dll", new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
  }
}