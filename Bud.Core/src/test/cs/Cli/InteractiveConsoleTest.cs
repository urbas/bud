using System;
using System.Linq;
using Bud.Cli.ConsoleTestUtils;
using Bud.Commander;
using Moq;
using NUnit.Framework;

namespace Bud.Cli {
  public class InteractiveConsoleTest {
    private Mock<IBuildContext> BuildContext;
    private Mock<IConsoleInput> ConsoleInput;
    private InteractiveConsole InteractiveConsole;
    private MemoryConsoleBuffer ConsoleBuffer;
    private Mock<ICommandEvaluator> CommandEvaluator;

    [SetUp]
    public void SetUp() {
      BuildContext = new Mock<IBuildContext>();
      ConsoleInput = new Mock<IConsoleInput>();
      ConsoleBuffer = new MemoryConsoleBuffer(20, 5);
      CommandEvaluator = new Mock<ICommandEvaluator>();
      InteractiveConsole = new InteractiveConsole(BuildContext.Object, ConsoleInput.Object, ConsoleBuffer, CommandEvaluator.Object);
    }

    [Test]
    public void Pressing_enter_should_clear_the_line() {
      EnterKeySequence(ConsoleKey.A, ConsoleKey.Enter);
      Assert.IsEmpty(InteractiveConsole.Editor.Line);
    }

    [Test]
    public void Pressing_up_WHEN_history_is_empty_MUST_do_nothing() {
      EnterKeySequence(ConsoleKey.A, ConsoleKey.UpArrow);
      Assert.AreEqual("A", InteractiveConsole.Editor.Line);
    }

    [Test]
    public void Pressing_up_should_show_the_last_command_in_history() {
      EnterKeySequence(ConsoleKey.A, ConsoleKey.Enter, ConsoleKey.UpArrow);
      Assert.AreEqual("A", InteractiveConsole.Editor.Line);
    }

    [Test]
    public void Pressing_up_twice_should_show_the_command_second_in_history() {
      EnterKeySequence(ConsoleKey.B, ConsoleKey.Enter, ConsoleKey.A, ConsoleKey.Enter, ConsoleKey.UpArrow, ConsoleKey.UpArrow);
      Assert.AreEqual("B", InteractiveConsole.Editor.Line);
    }

    [Test]
    public void Pressing_up_and_down_should_show_entered_command() {
      EnterKeySequence(ConsoleKey.B, ConsoleKey.Enter, ConsoleKey.A, ConsoleKey.UpArrow, ConsoleKey.DownArrow);
      Assert.AreEqual("A", InteractiveConsole.Editor.Line);
    }

    [Test]
    public void Pressing_up_and_invoking_the_command_MUST_store_it_as_the_last_entry() {
      EnterKeySequence(ConsoleKey.B, ConsoleKey.Enter, ConsoleKey.A, ConsoleKey.UpArrow, ConsoleKey.C, ConsoleKey.Enter, ConsoleKey.UpArrow);
      Assert.AreEqual("BC", InteractiveConsole.Editor.Line);
    }

    [Test]
    public void Pressing_up_and_invoking_the_command_MUST_not_change_the_old_history_entry() {
      EnterKeySequence(ConsoleKey.B, ConsoleKey.Enter, ConsoleKey.UpArrow, ConsoleKey.C, ConsoleKey.Enter, ConsoleKey.UpArrow, ConsoleKey.UpArrow);
      Assert.AreEqual("B", InteractiveConsole.Editor.Line);
    }

    private void EnterKeySequence(params ConsoleKey[] consoleKeys) {
      var keySequenceBuilder = ConsoleInput.SetupSequence(self => self.ReadKey());
      consoleKeys
        .Aggregate(keySequenceBuilder, (builder, consoleKey) => builder.Returns(ToConsoleKeyInfo(consoleKey)))
        .Returns(QuitKey);
      InteractiveConsole.Serve();
    }

    public static ConsoleKeyInfo QuitKey
      => ToConsoleKeyInfo(ConsoleKey.Q, ConsoleModifiers.Control);
  }
}