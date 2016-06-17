using Bud.References;
using NUnit.Framework;

namespace Bud.Scripting {
  public class BudReferenceResolverTest {
    private static readonly string OptionAssemblyName = typeof(Option).Assembly.GetName().Name;

    private static readonly Assembly OptionAssemblyReference
      = new Assembly(OptionAssemblyName, typeof(Option).Assembly.Location);

    [Test]
    public void Resolve_returns_Bud_Option_reference()
      => Assert.That(Resolve(OptionAssemblyName).Assemblies,
                     Does.Contain(OptionAssemblyReference));

    [Test]
    public void Resolve_deduplicates_references()
      => Assert.That(Resolve(OptionAssemblyName, OptionAssemblyName).Assemblies,
                     Is.EqualTo(new[] {OptionAssemblyReference}));

    [Test]
    public void Resolve_returns_references_of_Bud_Option()
      => Assert.That(new BudReferenceResolver().Resolve(new[] {OptionAssemblyName}).FrameworkAssemblies,
                     Does.Contain(new FrameworkAssembly("System.Core", FrameworkAssembly.MaxVersion)));

    [Test]
    public void Resolve_returns_transitive_references_of_Bud_Make()
      => Assert.That(new BudReferenceResolver().Resolve(new[] {"Bud.Make"}).Assemblies,
                     Does.Contain(OptionAssemblyReference));

    [Test]
    public void Resolve_returns_assemblies_found_on_filesystem() {
      using (var dir = new TmpDir()) {
        var dll = dir.CreateEmptyFile("A.dll");
        Assert.That(new BudReferenceResolver().Resolve(new[] {dll}).Assemblies,
                    Does.Contain(Assembly.FromPath(dll)));
      }
    }

    private static ResolvedReferences Resolve(params string[] references)
      => new BudReferenceResolver().Resolve(references);
  }
}