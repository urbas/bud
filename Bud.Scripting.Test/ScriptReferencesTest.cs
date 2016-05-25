using NUnit.Framework;
using static NUnit.Framework.Assert;

namespace Bud.Scripting {
  public class ScriptReferencesTest {
    [Test]
    public void Parse_returns_no_references_from_an_empty_string()
      => IsEmpty(ScriptReferences.Parse(""));

    [Test]
    public void Parse_returns_the_only_reference()
      => AreEqual(new[] {"System.Linq"},
                  ScriptReferences.Parse("//!reference System.Linq"));

    [Test]
    public void Parse_returns_no_references_if_reference_line_malformed()
      => IsEmpty(ScriptReferences.Parse("// reference System.Linq"));

    [Test]
    public void Parse_returns_references_until_first_non_commented_non_empty_line()
      => AreEqual(new[] {"Foo", "Bar", "Zar", "Moo"},
                  ScriptReferences.Parse(@"//!reference Foo
//!reference Bar
// This one is still in:
//!reference Zar

// So is this one:
//!reference Moo

var

// But not this one
//!reference Goo"));
  }
}