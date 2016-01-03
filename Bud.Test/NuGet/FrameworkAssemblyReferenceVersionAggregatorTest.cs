using System;
using System.Collections.Generic;
using NUnit.Framework;
using static Bud.NuGet.FrameworkAssemblyReferencesAggregator;
using static NUnit.Framework.Assert;

namespace Bud.NuGet {
  public class FrameworkAssemblyReferencesAggregatorTest {
    [Test]
    public void Returns_empty_when_given_no_references()
      => IsEmpty(AggregateReferences(new Tuple<string, Version>[] {}));

    [Test]
    public void Returns_the_same_list_when_there_are_no_duplicate_references()
      => AreEqual(new Dictionary<string, Version> {{"Foo", new Version(3, 5)}},
                  AggregateReferences(new[] {Tuple.Create("Foo", new Version(3, 5))}));

    [Test]
    public void Returns_a_list_with_removed_duplicate_references()
      => AreEqual(new Dictionary<string, Version> {{"Foo", new Version(3, 5)}},
                  AggregateReferences(new[] {
                    Tuple.Create("Foo", new Version(3, 5)),
                    Tuple.Create("Foo", new Version(3, 5))
                  }));

    [Test]
    public void Takes_the_higher_version_when_merging_duplicates()
      => AreEqual(new Dictionary<string, Version> {{"Foo", new Version(3, 5)}},
                  AggregateReferences(new[] {
                    Tuple.Create("Foo", new Version(3, 5)),
                    Tuple.Create("Foo", new Version(2, 0))
                  }));
  }
}