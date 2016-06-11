using System.Collections.Generic;
using NUnit.Framework;
using static Bud.Option;

namespace Bud.Scripting {
  public class BudReferenceResolverTest {
    private static readonly string OptionAssemblyName = typeof(Option).Assembly.GetName().Name;

    private static readonly KeyValuePair<string, Option<string>> OptionAssemblyReference
      = ToPair(OptionAssemblyName, typeof(Option).Assembly.Location);

    [Test]
    public void Resolve_unknown_reference()
      => Assert.That(new BudReferenceResolver().Resolve(new[] {"Foo.Bar"}),
                     Does.Contain(ToPair("Foo.Bar", None<string>())));

    [Test]
    public void Resolve_returns_Bud_Option_reference()
      => Assert.That(new BudReferenceResolver().Resolve(new[] {OptionAssemblyName}),
                     Does.Contain(OptionAssemblyReference));

    [Test]
    public void Resolve_returns_references_of_Bud_Option()
      => Assert.That(new BudReferenceResolver().Resolve(new[] {OptionAssemblyName}),
                     Does.Contain(ToPair("System.Core", None<string>())));

    [Test]
    public void Resolve_returns_transitive_references_of_Bud_Make()
      => Assert.That(new BudReferenceResolver().Resolve(new[] {"Bud.Make"}),
                     Does.Contain(OptionAssemblyReference));

    private static KeyValuePair<string, Option<string>> ToPair(string assemblyName,
                                                               Option<string> location)
      => new KeyValuePair<string, Option<string>>(assemblyName, location);
  }
}