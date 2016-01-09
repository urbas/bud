using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq.Expressions;
using Bud.IO;
using Microsoft.CodeAnalysis;
using Moq;
using NUnit.Framework;
using static System.IO.Path;
using static System.Linq.Enumerable;
using static Bud.Cs.CompileInputTestUtils;
using static Bud.IO.FileUtils;
using static NUnit.Framework.Assert;

namespace Bud.Cs {
  public class TimedEmittingCompilerTest {
    private Mock<ICompiler> underlyingCompiler;

    [SetUp]
    public void SetUp() => underlyingCompiler = new Mock<ICompiler>(MockBehavior.Strict);

    [Test]
    public void Do_not_invoke_underlying_compiler_when_ouput_dll_is_up_to_date() {
      using (var tmpDir = new TemporaryDirectory()) {
        var source = tmpDir.CreateEmptyFile("A.cs");
        tmpDir.CreateEmptyFile("Foo.dll");
        var compiler = CreateTimedEmittingCompiler(tmpDir.Path);

        compiler.Compile(ToCompileInput(source));
      }
    }

    [Test]
    public void Compile_returns_and_unsucessfull_compile_output_when_given_input_that_is_not_okay()
      => IsFalse(
        CreateTimedEmittingCompiler("dir")
          .Compile(ToCompileInput(dependency: UnsuccessfulCompileOutput()))
          .Success);

    [Test]
    public void Underlying_compiler_invoked_with_sources_assembly_references_and_dependencies() {
      using (var tmpDir = new TemporaryDirectory()) {
        var compiler = CreateTimedEmittingCompiler(tmpDir.Path);
        var sourceFile = tmpDir.CreateEmptyFile("A.cs");
        var dependency = FooDllCompileOutput();
        var referencedAssembly = tmpDir.CreateEmptyFile("Dep.dll");
        underlyingCompiler
          .Setup(
            self => self.Compile(It.Is(EqualToTimestampedFiles(sourceFile)),
                                 It.Is(EqualToTimestampedFiles(dependency.AssemblyPath, referencedAssembly)),
                                 compiler.OutputAssemblyPath))
          .Returns((CompileOutput) null);

        compiler.Compile(ToCompileInput(sourceFile, dependency, referencedAssembly));
        underlyingCompiler.VerifyAll();
      }
    }

    private TimedEmittingCompiler CreateTimedEmittingCompiler(string path)
      => new TimedEmittingCompiler(underlyingCompiler.Object,
                                   Combine(path, "Foo.dll"));

    private static Expression<Func<IEnumerable<Timestamped<string>>, bool>>
      EqualToTimestampedFiles(params string[] expectedFiles)
      => actualFiles => ToTimestampedFiles(expectedFiles)
                          .ToImmutableHashSet()
                          .SetEquals(actualFiles);

    private static CompileOutput UnsuccessfulCompileOutput()
      => new CompileOutput(Empty<Diagnostic>(), TimeSpan.Zero, "Foo.dll", false, 0L, null);

    private static CompileOutput FooDllCompileOutput()
      => new CompileOutput(Empty<Diagnostic>(), TimeSpan.Zero, "Foo.dll", true, 0L, null);
  }
}