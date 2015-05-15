using static Bud.Util.StringUtils;
using System;
using System.IO;
using NUnit.Framework;

namespace Bud.Util {
  public class StringUtilsTest {
    [Test]
    public void Capitalize_MUST_return_null_WHEN_given_null() {
      Assert.IsNull(Capitalize(null));
    }

    [Test]
    public void Capitalize_MUST_return_a_string_with_an_uppercase_first_letter_WHEN_given_a_single_lowercase_word() {
      Assert.AreEqual("Foo", Capitalize("foo"));
    }

    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ToHexString_MUST_throw_an_exception_WHEN_given_a_null_byte_array() {
      ToHexString(null, new StringWriter());
    }

    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ToHexString_MUST_throw_an_exception_WHEN_given_a_null_text_writer() {
      ToHexString(new byte[0], null);
    }

    [Test]
    public void ToHexString_MUST_write_nothing_WHEN_given_no_bytes() {
      Assert.AreEqual(string.Empty, ToHexString(new byte[0], new StringWriter()).ToString());
    }

    [Test]
    public void ToHexString_MUST_write_a_hex_string_WHEN_given_some_bytes() {
      Assert.AreEqual("0123456789abcdef",
                      ToHexString(new byte[] {0x01, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef}, new StringWriter()).ToString());
    }

    [Test]
    public void CommonPrefix_returns_empty_string_WHEN_given_an_empty_list() {
      Assert.IsEmpty(CommonPrefix());
    }

    [Test]
    public void CommonPrefix_returns_the_only_string_in_the_list() {
      Assert.AreEqual("foo", CommonPrefix("foo"));
    }

    [Test]
    public void CommonPrefix_returns_empty_string_WHEN_strings_in_list_do_not_share_a_common_prefix() {
      Assert.IsEmpty(CommonPrefix("foo", "bar"));
    }

    [Test]
    public void CommonPrefix_returns_part_of_the_first_word_WHEN_the_second_word_starts_with_that_part() {
      Assert.AreEqual("fo", CommonPrefix("foo", "fod"));
    }

    [Test]
    public void CommonPrefix_returns_the_shortest_word_WHEN_the_list_contains_progressively_smaller_words() {
      Assert.AreEqual("fo", CommonPrefix("foobar", "foob", "foe"));
      Assert.AreEqual("lo", CommonPrefix("love", "lov", "lo"));
    }
  }
}