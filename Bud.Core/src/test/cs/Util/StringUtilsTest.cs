using NUnit.Framework;

namespace Bud.Util {
  public class StringUtilsTest {
    [Test]
    public void Capitalize_MUST_return_null_WHEN_given_null() {
      Assert.IsNull(StringUtils.Capitalize(null));
    }

    [Test]
    public void Capitalize_MUST_return_a_string_with_an_uppercase_first_letter_WHEN_given_a_single_lowercase_word() {
      Assert.AreEqual("Foo", StringUtils.Capitalize("foo"));
    }
  }
}