using System;
using System.Collections.Immutable;
using Bud.IO;
using Moq;
using NUnit.Framework;

namespace Bud.Cs {
  public class CompileInputTest {
    private readonly ImmutableArray<Timestamped<string>> sources = ImmutableArray.Create(Timestamped.Create("foo", 1));
    private readonly ImmutableArray<Timestamped<IAssemblyReference>> assemblies = ImmutableArray.Create(Timestamped.Create(new Mock<IAssemblyReference>().Object, 1));
    private readonly ImmutableArray<CompileOutput> dependencies = ImmutableArray.Create(new CompileOutput(null, TimeSpan.MinValue, "foo", false, 1L, null));
    private CompileInput compileInput;

    [SetUp]
    public void SetUp()
      => compileInput = new CompileInput(sources, assemblies, dependencies);

    [Test]
    public void Sources_assemblies_and_dependencies() {
      Assert.AreEqual(sources, compileInput.Sources);
      Assert.AreEqual(assemblies, compileInput.Assemblies);
      Assert.AreEqual(dependencies, compileInput.Dependencies);
    }

    [Test]
    public void CompileInput_equals()
      => Assert.AreEqual(new CompileInput(sources, assemblies, dependencies),
                         compileInput);

    [Test]
    public void CompileInput_does_not_equal()
      => Assert.AreNotEqual(new CompileInput(sources, ImmutableArray<Timestamped<IAssemblyReference>>.Empty, dependencies),
                            compileInput);

    [Test]
    public void CompileInput_hashes_equal()
      => Assert.AreEqual(new CompileInput(sources, assemblies, dependencies).GetHashCode(),
                         compileInput.GetHashCode());
  }
}