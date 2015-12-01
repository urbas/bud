using System.Collections.Immutable;
using System.IO;
using Bud.IO;
using Microsoft.CodeAnalysis;
using Moq;
using NUnit.Framework;

namespace Bud.Cs {
  public class TimedEmittingCompilerTest {
    private TimedEmittingCompiler compiler;
    private Mock<ICompiler> underlyingCompiler;

    [SetUp]
    public void SetUp() {
      underlyingCompiler = new Mock<ICompiler>();
      compiler = new TimedEmittingCompiler(ImmutableList<ResourceDescription>.Empty, underlyingCompiler.Object, Path.Combine("foo", "Foo.dll"));
    }

    [Test]
    public void Compile_returns_and_unsucessfull_compile_output_when_given_input_that_is_not_okay() {
      var compilerInput = new InOut(Assembly.ToAssembly("A.dll", false));
      Assert.IsFalse(compiler.Compile(compilerInput).Success);
    }
  }
}