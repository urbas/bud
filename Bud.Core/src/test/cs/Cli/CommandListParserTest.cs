using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Bud.Cli {
  public class CommandListParserTest {
    [Test]
    public void ToCommandList_MUST_return_an_empty_list_WHEN_given_null() {
      Assert.IsEmpty(CommandListParser.ToCommandList(null));
    }

    [Test]
    public void ToCommandList_MUST_return_a_key_command_WHEN_given_a_single_key() {
      AssertCommandsAreEqual(new KeyCommand("foo"),
                             CommandListParser.ToCommandList(new[] {"foo"}).First());
    }

    [Test]
    public void ToCommandList_MUST_return_a_macro_command_WHEN_given_a_single_macro() {
      AssertCommandsAreEqual(new MacroCommand("foo"),
                             CommandListParser.ToCommandList(new[] {"@foo"}).First());
    }

    [Test]
    public void ToCommandList_MUST_return_a_macro_command_with_parameters_WHEN_given_macro_parameters() {
      AssertCommandsAreEqual(new MacroCommand("foo", "par1", "par2"),
                             CommandListParser.ToCommandList(new[] {"@foo", "par1", "par2"}).First());
    }

    [Test]
    public void ToCommandList_MUST_return_a_list_of_key_commands_WHEN_given_only_keys() {
      AssertCommandsAreEqual(new[] {new KeyCommand("foo"), new KeyCommand("bar")},
                             CommandListParser.ToCommandList(new[] {"foo", "bar"}));
    }

    [Test]
    public void ToCommandList_MUST_return_a_list_of_macro_commands_WHEN_given_only_macros() {
      AssertCommandsAreEqual(new[] {new MacroCommand("foo"), new MacroCommand("bar")},
                             CommandListParser.ToCommandList(new[] {"@foo", "@bar"}));
    }

    private void AssertCommandsAreEqual(IEnumerable<Command> expectedCommands,
                                        IEnumerable<Command> actualCommands) {
      Assert.AreEqual(expectedCommands.Count(), actualCommands.Count(), "Number of commands does not match.");
      expectedCommands.Zip(actualCommands, (expectedCommand, actualCommand) => new {expectedCommand, actualCommand})
                      .All(commandPairs => AssertCommandsAreEqual(commandPairs.expectedCommand, commandPairs.actualCommand));
    }

    private bool AssertCommandsAreEqual(Command expectedCommand, Command actualCommand) {
      if (expectedCommand is MacroCommand) {
        AssertCommandsAreEqual((MacroCommand) expectedCommand, (MacroCommand) actualCommand);
      } else {
        Assert.AreEqual(((KeyCommand) expectedCommand).Key, ((KeyCommand) actualCommand).Key);
      }
      return true;
    }

    private static void AssertCommandsAreEqual(MacroCommand expectedCommand, MacroCommand actualCommand) {
      Assert.AreEqual(expectedCommand.MacroName, actualCommand.MacroName);
      Assert.AreEqual(expectedCommand.Parameters, actualCommand.Parameters);
    }
  }
}