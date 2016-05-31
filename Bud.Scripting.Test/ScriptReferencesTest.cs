using System;
using NUnit.Framework;
using static Bud.Option;
using static NUnit.Framework.Assert;

namespace Bud.Scripting {
  public class ScriptReferencesTest {
    [Test]
    public void Extract_returns_no_references_from_an_empty_string()
      => IsEmpty(ScriptReferences.Extract(""));

    [Test]
    public void Extract_returns_the_only_reference()
      => AreEqual(new[] {new ScriptReference("System.Linq", None<string>())},
                  ScriptReferences.Extract("//!reference System.Linq"));

    [Test]
    public void Extract_from_multiple_files()
      => That(ScriptReferences.Extract(new[] {"//!reference Foo", "//!reference Bar"}),
              Is.EquivalentTo(new[] {
                new ScriptReference("Foo", None<string>()),
                new ScriptReference("Bar", None<string>()),
              }));

    [Test]
    public void Extract_returns_no_references_if_reference_line_malformed()
      => IsEmpty(ScriptReferences.Extract("// reference System.Linq"));

    [Test]
    public void Extract_returns_references_until_first_non_commented_non_empty_line()
      => That(ScriptReferences.Extract(@"//!reference Foo
//!reference Bar
// This one is still in:
//!reference Zar

// So is this one:
//!reference Moo

var

// But not this one
//!reference Goo"),
              Is.EquivalentTo(new[] {
                new ScriptReference("Foo", None<string>()),
                new ScriptReference("Bar", None<string>()),
                new ScriptReference("Zar", None<string>()),
                new ScriptReference("Moo", None<string>())
              }));

    [Test]
    public void Extract_returns_versioned_references()
      => AreEqual(new[] {new ScriptReference("Foo", "1.2.3")},
                  ScriptReferences.Extract(@"//!reference Foo 1.2.3"));

    [Test]
    public void Extract_throws_when_given_invalid_reference()
      => Throws<Exception>(() => ScriptReferences.Extract(@"//!reference Foo 1.2.3 blah"));
  }
}