using System;
using System.Collections.Immutable;
using NUnit.Framework;
using static Bud.Option;
using static NUnit.Framework.Assert;

namespace Bud.Scripting {
  public class ScriptMetadataTest {
    [Test]
    public void Extract_returns_empty_assembly_references_when_given_an_empty_script()
      => IsEmpty(ScriptMetadata.Extract(string.Empty).AssemblyReferences);

    [Test]
    public void Extract_returns_specified_assembly_references()
      => AreEqual(new[] {"Foo"},
                  ScriptMetadata.Extract(@"//!reference Foo").AssemblyReferences);

    [Test]
    public void Extract_returns_no_references_if_reference_line_malformed()
      => IsEmpty(ScriptMetadata.Extract("// reference System.Linq").AssemblyReferences);

    [Test]
    public void Extract_returns_references_until_first_non_commented_non_empty_line()
      => That(ScriptMetadata.Extract(@"//!reference Foo
//!reference Bar
// This one is still in:
//!reference Zar

// So is this one:
//!reference Moo

var

// But not this one
//!reference Goo").AssemblyReferences,
              Is.EquivalentTo(new[] {"Foo", "Bar", "Zar","Moo"}));

    [Test]
    public void Extract_returns_nuget_package_references()
      => AreEqual(ImmutableDictionary<string, Option<string>>.Empty.Add("Foo", None<string>()),
                  ScriptMetadata.Extract(@"//!nuget Foo").NuGetReferences);

    [Test]
    public void Extract_returns_versioned_nuget_package_references()
      => AreEqual(ImmutableDictionary<string, Option<string>>.Empty.Add("Foo", Some("1.2.3")),
                  ScriptMetadata.Extract(@"//!nuget Foo 1.2.3").NuGetReferences);

    [Test]
    public void Extract_throws_when_given_an_invalid_reference()
      => Throws<Exception>(() => ScriptMetadata.Extract(@"//!reference Foo 1.2.3"));

    [Test]
    public void Extract_throws_when_given_an_invalid_nuget_reference()
      => Throws<Exception>(() => ScriptMetadata.Extract(@"//!nuget Foo 1.2.3 blah"));
  }
}