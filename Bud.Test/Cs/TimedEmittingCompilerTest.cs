using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Bud.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Moq;
using NUnit.Framework;

namespace Bud.Cs {
  public class TimedEmittingCompilerTest {
    private Mock<ICompiler> underlyingCompiler;

    [SetUp]
    public void SetUp() => underlyingCompiler = new Mock<ICompiler>(MockBehavior.Strict);

    [Test]
    public void Compile_returns_and_unsucessfull_compile_output_when_given_input_that_is_not_okay() {
      var compiler = new TimedEmittingCompiler(ImmutableList<ResourceDescription>.Empty, underlyingCompiler.Object, Path.Combine("foo", "Foo.dll"));
      var compilerInput = new InOut(UnsuccessfulCompileOutput());
      Assert.IsFalse(compiler.Compile(compilerInput).Success);
    }

    [Test]
    public void Underlying_compiler_invoked_with_dependencies() {
      using (var tmpDir = new TemporaryDirectory()) {
        var compiler = new TimedEmittingCompiler(ImmutableList<ResourceDescription>.Empty, underlyingCompiler.Object, Path.Combine(tmpDir.Path, "Foo.dll"));
        var compilerInput = new InOut(FooDllCompileOutput());
        var inputAssemblies = new[] {Timestamped.Create("Foo.dll", 0L)};
        underlyingCompiler.Setup(self => self.Compile(It.IsAny<IEnumerable<Timestamped<string>>>(),
                                                      inputAssemblies))
                                                      .Returns((CSharpCompilation) null);
        Assert.That(() => compiler.Compile(compilerInput), Throws.TypeOf<Exception>());
        underlyingCompiler.VerifyAll();
      }
    }

    [Test]
    public void Do_not_invoke_underlying_compiler_when_ouput_dll_is_up_to_date() {
      using (var tmpDir = new TemporaryDirectory()) {
        var source = tmpDir.CreateEmptyFile("A.cs");
        tmpDir.CreateEmptyFile("A.dll");
        var compiler = new TimedEmittingCompiler(ImmutableList<ResourceDescription>.Empty, underlyingCompiler.Object, Path.Combine(tmpDir.Path, "A.dll"));
        compiler.Compile(new InOut(source));
        underlyingCompiler.Verify(self => self.Compile(It.IsAny<IEnumerable<Timestamped<string>>>(), It.IsAny<IEnumerable<Timestamped<string>>>()), Times.Never);
      }
    }

    [Test]
    public void Invoke_underlying_compiler_when_ouput_dll_is_not_up_to_date() {
      using (var tmpDir = new TemporaryDirectory()) {
        var source = tmpDir.CreateEmptyFile("A.cs");
        var assemblyReference = tmpDir.CreateEmptyFile("Foo.dll");
        underlyingCompiler.Setup(self => self.Compile(It.Is(EqualToTimestampedFiles(source)), It.Is(EqualToTimestampedFiles(assemblyReference)))).Returns(CSharpCompilation.Create("A.dll"));
        var compiler = new TimedEmittingCompiler(ImmutableList<ResourceDescription>.Empty, underlyingCompiler.Object, Path.Combine(tmpDir.Path, "A.dll"));
        compiler.Compile(new InOut(source, Assembly.ToAssembly(assemblyReference)));
        underlyingCompiler.VerifyAll();
      }
    }

    private Expression<Func<IEnumerable<Timestamped<string>>, bool>> EqualToTimestampedFiles(string expectedFile)
      => actualFiles => actualFiles.SequenceEqual(new[] {Files.ToTimestampedFile(expectedFile)});

    private static CompileOutput UnsuccessfulCompileOutput()
      => new CompileOutput(Enumerable.Empty<Diagnostic>(), TimeSpan.Zero, "Foo.dll", false, 0L, null);

    private static CompileOutput FooDllCompileOutput()
      => new CompileOutput(Enumerable.Empty<Diagnostic>(), TimeSpan.Zero, "Foo.dll", true, 0L, null);
  }
}