using System.IO;
using Bud.TempDir;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using NUnit.Framework;
using static System.Linq.Enumerable;

namespace Bud.Cs {
  [Category("IntegrationTest")]
  public class RoslynCsCompilationTest {
    [Test]
    public void Fails_to_compile_when_core_assembly_references_are_missing() {
      using (var tempDir = new TemporaryDirectory()) {
        var compiler = AssemblyACompiler();
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
        var compiler = AssemblyACompiler();
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
        var compiler = AssemblyACompiler();
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
    public void Fails_and_then_succeeds_to_compile_when_source_file_changes() {
      using (var tempDir = new TemporaryDirectory()) {
        var compiler = AssemblyACompiler();
        var fileA = tempDir.CreateFile("public class", "A.cs");
        var outputPath = Path.Combine(tempDir.Path, "A.dll");
        Assert.IsFalse(compiler.Compile(new[] {fileA}, new[] {typeof(object).Assembly.Location}).Emit(outputPath).Success);
        File.WriteAllText(fileA, "public class A {}");
        Assert.IsTrue(compiler.Compile(new[] {fileA}, new[] {typeof(object).Assembly.Location}).Emit(outputPath).Success);
      }
    }

    [Test]
    public void Fails_and_then_succeeds_to_compile_when_System_Runtime_assembly_introduced() {
      using (var tempDir = new TemporaryDirectory()) {
        var fileA = tempDir.CreateFile("public class A {}", "A.cs");
        var compiler = AssemblyACompiler();
        var outputPath = Path.Combine(tempDir.Path, "A.dll");
        var compilationA = compiler.Compile(new[] {fileA}, Empty<string>());
        Assert.IsFalse(compilationA.Emit(outputPath).Success);
        compilationA = compiler.Compile(new[] {fileA}, new[] {typeof(object).Assembly.Location});
        Assert.IsTrue(compilationA.Emit(outputPath).Success);
      }
    }

    [Test]
    public void Fails_to_compile_when_dependency_is_missing() {
      using (var tempDir = new TemporaryDirectory()) {
        var emitResult = CompileAssemblyB_UsingClassA(tempDir);
        Assert.IsFalse(emitResult.Success);
      }
    }

    [Test]
    public void Compiles_when_dependency_is_present() {
      using (var tempDir = new TemporaryDirectory()) {
        var assemblyA = CompileAssemblyA_WithPublicClassA(tempDir);
        var emitResult = CompileAssemblyB_UsingClassA(tempDir, assemblyA);
        Assert.IsTrue(emitResult.Success);
      }
    }

    [Test]
    public void Compiles_when_internals_of_dependency_are_used() {
      using (var tempDir = new TemporaryDirectory()) {
        var assemblyA = CompileAssemblyA_InternalsVisibleToB(tempDir);
        var emitResult = CompileAssemblyB_UsingInternalsOfA(tempDir, assemblyA);
        Assert.IsTrue(emitResult.Success);
      }
    }

    private static RoslynCsCompilation AssemblyACompiler()
      => new RoslynCsCompilation("A", new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    private static RoslynCsCompilation AssemblyBCompiler()
      => new RoslynCsCompilation("B", new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    private static string CompileAssemblyA_WithPublicClassA(TemporaryDirectory tempDir) {
      var classA = tempDir.CreateFile("public class A {}", "A.cs");
      var assemblyA = Path.Combine(tempDir.Path, "A.dll");
      AssemblyACompiler()
        .Compile(new[] {classA}, new[] {typeof(object).Assembly.Location})
        .Emit(assemblyA);
      return assemblyA;
    }

    private static EmitResult CompileAssemblyB_UsingClassA(TemporaryDirectory tempDir) {
      var fileB = tempDir.CreateFile("public class B : A {}", "B.cs");
      var outputPath = Path.Combine(tempDir.Path, "B.dll");
      return AssemblyBCompiler()
        .Compile(new[] {fileB}, new[] {typeof(object).Assembly.Location})
        .Emit(outputPath);
    }

    private static EmitResult CompileAssemblyB_UsingClassA(TemporaryDirectory tempDir, string assemblyA) {
      var fileB = tempDir.CreateFile("public class B : A {}", "B.cs");
      var outputPath = Path.Combine(tempDir.Path, "B.dll");
      return AssemblyBCompiler()
        .Compile(new[] {fileB}, new[] {typeof(object).Assembly.Location, assemblyA})
        .Emit(outputPath);
    }

    private static EmitResult CompileAssemblyB_UsingInternalsOfA(TemporaryDirectory tempDir, string assemblyA) {
      var classB = tempDir.CreateFile("public class B {int I=>A.I;}", "B.cs");
      return AssemblyBCompiler()
        .Compile(new[] {classB}, new[] {typeof(object).Assembly.Location, assemblyA})
        .Emit(Path.Combine(tempDir.Path, "B.dll"));
    }

    private static string CompileAssemblyA_InternalsVisibleToB(TemporaryDirectory tempDir) {
      var classA = tempDir.CreateFile("internal class A {internal const int I=42;}", "A.cs");
      var assemblyInfo = tempDir.CreateFile("using System.Runtime.CompilerServices;\n" +
                                            "[assembly: InternalsVisibleTo(\"B\")]", "AssemblyInfo.cs");
      var assemblyA = Path.Combine(tempDir.Path, "A.dll");
      AssemblyACompiler()
        .Compile(new[] {classA, assemblyInfo}, new[] {typeof(object).Assembly.Location})
        .Emit(assemblyA);
      return assemblyA;
    }
  }
}