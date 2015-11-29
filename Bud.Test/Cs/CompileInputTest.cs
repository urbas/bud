using System.Collections.Immutable;
using Bud.IO;
using Moq;
using NUnit.Framework;

namespace Bud.Cs {
  public class CompileInputTest {
    private readonly ImmutableArray<Timestamped<string>> sources = ImmutableArray.Create(Timestamped.Create("foo", 1));
    private readonly ImmutableArray<Timestamped<string>> assemblies = ImmutableArray.Create(Timestamped.Create("Foo.Bar.dll", 1));
    private CompileInput compileInput;

    [SetUp]
    public void SetUp()
      => compileInput = new CompileInput(sources, assemblies);

    [Test]
    public void Sources_assemblies_and_dependencies() {
      Assert.AreEqual(sources, compileInput.Sources);
      Assert.AreEqual(assemblies, compileInput.Assemblies);
    }

    [Test]
    public void CompileInput_equals()
      => Assert.AreEqual(new CompileInput(sources, assemblies),
                         compileInput);

    [Test]
    public void CompileInput_does_not_equal()
      => Assert.AreNotEqual(new CompileInput(sources, ImmutableArray<Timestamped<string>>.Empty),
                            compileInput);

    [Test]
    public void CompileInput_hashes_equal()
      => Assert.AreEqual(new CompileInput(sources, assemblies).GetHashCode(),
                         compileInput.GetHashCode());
  }
}