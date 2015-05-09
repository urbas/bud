using System;
using Moq;
using NUnit.Framework;

namespace Bud.Cli {
  public class SingleLineEditorTest {
    private SingleLineEditor SingleLineEditor;
    private MemoryConsoleBuffer ConsoleBuffer;
    private readonly ConsoleKey[] NavigationKeys = {ConsoleKey.PageUp, ConsoleKey.PageDown, ConsoleKey.Home, ConsoleKey.End, ConsoleKey.LeftArrow, ConsoleKey.UpArrow, ConsoleKey.RightArrow, ConsoleKey.DownArrow};

    [SetUp]
    public void SetUp() {
      ConsoleBuffer = new MemoryConsoleBuffer();
      SingleLineEditor = new SingleLineEditor(ConsoleBuffer);
    }

    [Test]
    public void Input_nothing_MUST_return_an_empty_line() {
      Assert.IsEmpty(SingleLineEditor.Line);
    }

    [Test]
    public void Input_a_character_MUST_return_single_character_line() {
      Input("a");
      Assert.AreEqual("a", SingleLineEditor.Line);
    }

    [Test]
    public void Input_a_character_MUST_write_the_character_to_the_console_buffer() {
      Input("a");
      Assert.AreEqual('a', ConsoleBuffer[0,0]);
    }

    [Test]
    public void Input_backspace_MUST_remote_the_character_from_the_line() {
      Input("a");
      Input(ConsoleKey.Backspace);
      Assert.IsEmpty(SingleLineEditor.Line);
    }

    [Test]
    public void Input_backspace_WHEN_line_is_empty_MUST_not_interact_with_console_buffer() {
      Input(ConsoleKey.Backspace);
      VerifyConsoleBufferHasInitialState();
    }

    [Test]
    public void Input_navigation_keys_WHEN_line_is_empty_MUST_not_interact_with_console_buffer() {
      Input(NavigationKeys);
      VerifyConsoleBufferHasInitialState();
    }

    [Test]
    public void Input_character_A_AND_left_AND_character_B_MUST_return_line_BA() {
      Input(ConsoleKey.A, ConsoleKey.LeftArrow, ConsoleKey.B);
      Assert.AreEqual("BA", SingleLineEditor.Line);
    }

    [Test]
    public void Input_some_character_AND_left_MUST_move_the_console_buffer_cursor_back() {
      Input(ConsoleKey.A);
      Assert.AreEqual(1, ConsoleBuffer.CursorLeft);
      Input(ConsoleKey.LeftArrow);
      Assert.AreEqual(0, ConsoleBuffer.CursorLeft);
    }

    [Test]
    public void Input_character_after_left_MUST_move_the_trailing_characters_to_right() {
      Input(ConsoleKey.A, ConsoleKey.LeftArrow, ConsoleKey.C);
      Assert.AreEqual(ToBuffer("CA"), ConsoleBuffer.ToArray());
    }

    [Test]
    public void deleting_characters_in_the_middle_MUST_return_the_edited_line() {
      Input("This is madness, right?");
      Input(ConsoleKey.LeftArrow, 8);
      Input(ConsoleKey.Backspace, 7);
      Input("Sparta");
      Assert.AreEqual("This is Sparta, right?", SingleLineEditor.Line);
    }

    [Test]
    public void deleting_characters_in_the_middle_MUST_move_the_buffer() {
      Input("012345");
      Input(ConsoleKey.LeftArrow, ConsoleKey.LeftArrow, ConsoleKey.Backspace);
      Assert.AreEqual(ToBuffer("01245"), ConsoleBuffer.ToArray());
    }

    [Test]
    public void deleting_characters_at_the_end_MUST_clear_the_trailing_character() {
      Input(ConsoleKey.A, ConsoleKey.Backspace);
      Assert.AreEqual('\0', ConsoleBuffer[0, 0]);
    }

    private void Input(ConsoleKeyInfo consoleKey, int repeatCount) {
      for (int i = 0; i < repeatCount; i++) {
        SingleLineEditor.ProcessInput(consoleKey);
      }
    }

    private void Input(params ConsoleKey[] consoleKeys) {
      foreach (var consoleKey in consoleKeys) {
        Input(consoleKey);
      }
    }

    private void Input(ConsoleKey consoleKey, int repeatCount) {
      Input(ToInput(consoleKey), repeatCount);
    }

    private void Input(string str) {
      foreach (var chr in str) {
        Input(new ConsoleKeyInfo(chr, (ConsoleKey) char.ToUpperInvariant(chr), false, false, false), 1);
      }
    }

    private void Input(ConsoleKey consoleKey) => Input(ToInput(consoleKey), 1);

    private static ConsoleKeyInfo ToInput(ConsoleKey consoleKey) {
      return new ConsoleKeyInfo((char) consoleKey, consoleKey, false, false, false);
    }

    private void VerifyConsoleBufferHasInitialState() {
      Assert.That(ConsoleBuffer.ToArray(), Is.All.EqualTo('\0'));
      Assert.AreEqual(0, ConsoleBuffer.CursorLeft);
      Assert.AreEqual(0, ConsoleBuffer.CursorTop);
    }

    private char[] ToBuffer(string prefix) {
      var buffer = new char[ConsoleBuffer.BufferWidth * ConsoleBuffer.BufferHeight];
      Array.Copy(prefix.ToCharArray(), buffer, prefix.Length);
      return buffer;
    }
  }
}