using System;
using System.Linq;
using NUnit.Framework;
using static NUnit.Framework.Assert;

namespace Bud.References {
  public class ResolvedReferencesTest {
    private readonly ResolvedReferences refsA = new ResolvedReferences(new[] {Assembly.ToAssemblyFile("A.dll"),},
                                                                       new [] {new FrameworkAssembly("System", Version.Parse("4.6.0"))});

    private readonly ResolvedReferences refsB = new ResolvedReferences(new[] {Assembly.ToAssemblyFile("B.dll"),},
                                                                       new[] { new FrameworkAssembly("System.Data", Version.Parse("4.6.0")) });

    [Test]
    public void Add_empty_to_any_returns_unchanged()
      => AreEqual(refsA, refsA.Add(ResolvedReferences.Empty));

    [Test]
    public void Add_some_returns_concatenated_assemblies()
      => AreEqual(new ResolvedReferences(refsA.Assemblies.Concat(refsB.Assemblies),
                                         refsA.FrameworkAssemblies.Concat(refsB.FrameworkAssemblies)),
                  refsA.Add(refsB));
  }
}