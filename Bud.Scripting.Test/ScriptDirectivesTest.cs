using System;
using System.Collections.Immutable;
using NUnit.Framework;
using static Bud.Option;
using static NUnit.Framework.Assert;

namespace Bud.Scripting {
  public class ScriptDirectivesTest {
    [Test]
    public void Extract_returns_empty_assembly_references_when_given_an_empty_script()
      => IsEmpty(ScriptDirectives.Extract(string.Empty).References);

    [Test]
    public void Extract_returns_specified_assembly_references()
      => AreEqual(new[] {"Foo"},
                  ScriptDirectives.Extract(@"//!reference Foo").References);

    [Test]
    public void Extract_deduplicates_assembly_references()
      => AreEqual(new[] {"Foo"},
                  ScriptDirectives.Extract(@"//!reference Foo", @"//!reference Foo").References);

    [Test]
    public void Extract_returns_no_references_if_reference_line_malformed()
      => IsEmpty(ScriptDirectives.Extract("// reference System.Linq").References);

    [Test]
    public void Extract_returns_references_until_first_non_commented_non_empty_line()
      => That(ScriptDirectives.Extract(@"//!reference Foo
//!reference Bar
// This one is still in:
//!reference Zar

// So is this one:
//!reference Moo

var

// But not this one
//!reference Goo").References,
              Is.EquivalentTo(new[] {"Foo", "Bar", "Zar","Moo"}));

    [Test]
    public void Extract_returns_nuget_package_references()
      => AreEqual(ImmutableDictionary<string, Option<string>>.Empty.Add("Foo", None<string>()),
                  ScriptDirectives.Extract(@"//!nuget Foo").NuGetReferences);

    [Test]
    public void Extract_returns_versioned_nuget_package_references()
      => AreEqual(ImmutableDictionary<string, Option<string>>.Empty.Add("Foo", Some("1.2.3")),
                  ScriptDirectives.Extract(@"//!nuget Foo 1.2.3").NuGetReferences);

    [Test]
    public void Extract_throws_when_given_an_invalid_reference()
      => Throws<Exception>(() => ScriptDirectives.Extract(@"//!reference Foo 1.2.3"));

    [Test]
    public void Extract_throws_when_given_an_invalid_nuget_reference()
      => Throws<Exception>(() => ScriptDirectives.Extract(@"//!nuget Foo 1.2.3 blah"));
  }
}