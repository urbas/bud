using System;
using NUnit.Framework;

namespace Bud.References {
  public class AssemblyAggregatorTest {
    [Test]
    public void Returns_empty_when_given_no_references()
      => Assert.IsEmpty(AssemblyCollections.DeduplicateAssemblies(new FrameworkAssembly[] {}));

    [Test]
    public void Returns_the_same_list_when_there_are_no_duplicate_references()
      => Assert.AreEqual(new[] {new FrameworkAssembly("Foo", new Version(3, 5))},
                  AssemblyCollections.DeduplicateAssemblies(new[] {
                    new FrameworkAssembly("Foo", new Version(3, 5))
                  }));

    [Test]
    public void Returns_a_list_with_removed_duplicate_references()
      => Assert.AreEqual(new[] {new FrameworkAssembly("Foo", new Version(3, 5))},
                  AssemblyCollections.DeduplicateAssemblies(new[] {
                    new FrameworkAssembly("Foo", new Version(3, 5)),
                    new FrameworkAssembly("Foo", new Version(3, 5))
                  }));

    [Test]
    public void Takes_the_higher_version_when_merging_duplicates()
      => Assert.AreEqual(new[] {new FrameworkAssembly("Foo", new Version(3, 5))},
                  AssemblyCollections.DeduplicateAssemblies(new[] {
                    new FrameworkAssembly("Foo", new Version(3, 5)),
                    new FrameworkAssembly("Foo", new Version(2, 0))
                  }));
  }
}