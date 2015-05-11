using Bud.Cli.CommandCompletions;
using NUnit.Framework;

namespace Bud.Cli {
  public class CommandCompletionsTest {
    [Test]
    public void ExtractPartialCommand_MUST_return_an_empty_string_WHEN_cursor_is_at_the_beginning() {
      Assert.IsEmpty(ExtractPartialCommand("build", 0));
    }

    [Test]
    public void ExtractPartialCommand_MUST_return_a_part_of_the_command_WHEN_cursor_is_within_the_command() {
      Assert.AreEqual("bui", ExtractPartialCommand("build", 3));
    }

    [Test]
    public void ExtractPartialCommand_MUST_omit_the_leading_space() {
      Assert.AreEqual("bui", ExtractPartialCommand(" bui", 3));
    }
  }
}