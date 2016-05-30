using NUnit.Framework;
using static NUnit.Framework.Assert;

namespace Bud.Scripting {
  public class ScriptReferencesTest {
    [Test]
    public void Extract_returns_no_references_from_an_empty_string()
      => IsEmpty(ScriptReferences.Extract(""));

    [Test]
    public void Extract_returns_the_only_reference()
      => AreEqual(new[] {"System.Linq"},
                  ScriptReferences.Extract("//!reference System.Linq"));

    [Test]
    public void Extract_from_multiple_files()
      => That(ScriptReferences.Extract(new[] {"//!reference Foo", "//!reference Bar"}),
              Is.EquivalentTo(new[] {"Foo", "Bar"}));

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
              Is.EquivalentTo(new[] {"Foo", "Bar", "Zar", "Moo"}));
  }
}