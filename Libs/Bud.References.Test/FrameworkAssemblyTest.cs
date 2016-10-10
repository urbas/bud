using System;
using NUnit.Framework;

namespace Bud.References {
  public class FrameworkAssemblyTest {
    [Test]
    public void TakeHighestVersions_returns_empty_when_given_no_references()
      => Assert.IsEmpty(FrameworkAssembly.TakeHighestVersions(new FrameworkAssembly[] {}));

    [Test]
    public void TakeHighestVersions_returns_the_same_list_when_there_are_no_duplicate_references()
      => Assert.AreEqual(new[] {new FrameworkAssembly("Foo", new Version(3, 5))},
                  FrameworkAssembly.TakeHighestVersions(new[] {
                    new FrameworkAssembly("Foo", new Version(3, 5))
                  }));

    [Test]
    public void TakeHighestVersions_returns_a_list_with_removed_duplicate_references()
      => Assert.AreEqual(new[] {new FrameworkAssembly("Foo", new Version(3, 5))},
                  FrameworkAssembly.TakeHighestVersions(new[] {
                    new FrameworkAssembly("Foo", new Version(3, 5)),
                    new FrameworkAssembly("Foo", new Version(3, 5))
                  }));

    [Test]
    public void TakeHighestVersions_retains_the_higher_version_when_merging_duplicates()
      => Assert.AreEqual(new[] {new FrameworkAssembly("Foo", new Version(3, 5))},
                  FrameworkAssembly.TakeHighestVersions(new[] {
                    new FrameworkAssembly("Foo", new Version(3, 5)),
                    new FrameworkAssembly("Foo", new Version(2, 0))
                  }));
  }
}