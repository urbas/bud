using System;
using System.Linq;
using NUnit.Framework;

namespace Bud.Cli {
  public class SingleLineEditorTest {
    private const string TwoRowLine = "This is a two-row line. I am not lying.";
    private SingleLineEditor SingleLineEditor;
    private MemoryConsoleBuffer ConsoleBuffer;

    [SetUp]
    public void SetUp() => SetCursorPosition(0, 0);

    [Test]
    public void Input_nothing_MUST_return_an_empty_line() {
      Assert.IsEmpty(SingleLineEditor.Line);
    }

    [Test]
    public void Initial_cursor_position_must_be_0() {
      Assert.AreEqual(0, SingleLineEditor.CursorPosition);
    }

    [Test]
    public void WHEN_single_character_entered_THEN_cursor_position_must_be_1() {
      Input("a");
      Assert.AreEqual(1, SingleLineEditor.CursorPosition);
    }

    [Test]
    public void Input_a_character_MUST_return_single_character_line() {
      Input("a");
      Assert.AreEqual("a", SingleLineEditor.Line);
    }

    [Test]
    public void Input_a_character_MUST_write_the_character_to_the_console_buffer() {
      Input("a");
      Assert.AreEqual('a', ConsoleBuffer[0, 0]);
    }

    [Test]
    public void Input_backspace_MUST_remove_the_character_from_the_line() {
      Input("a");
      Input(ConsoleKey.Backspace);
      Assert.IsEmpty(SingleLineEditor.Line);
    }

    [Test]
    public void WHEN_removing_single_character_THEN_cursor_position_must_be_0() {
      Input(ConsoleKey.A, ConsoleKey.Backspace);
      Assert.AreEqual(0, SingleLineEditor.CursorPosition);
    }

    [Test]
    public void Input_backspace_WHEN_line_is_empty_MUST_not_interact_with_console_buffer() {
      Input(ConsoleKey.Backspace);
      VerifyConsoleBufferHasInitialState();
    }

    [Test]
    public void Input_navigation_keys_WHEN_line_is_empty_MUST_not_interact_with_console_buffer() {
      Input(ConsoleKey.Home, ConsoleKey.End, ConsoleKey.LeftArrow, ConsoleKey.RightArrow);
      VerifyConsoleBufferHasInitialState();
    }

    [Test]
    public void Input_character_A_AND_left_AND_character_B_MUST_return_line_BA() {
      Input(ConsoleKey.A, ConsoleKey.LeftArrow, ConsoleKey.B);
      Assert.AreEqual("BA", SingleLineEditor.Line);
    }

    [Test]
    public void Input_character_AND_go_left_AND_input_character_MUST_set_cursor_position_to_1() {
      Input(ConsoleKey.A, ConsoleKey.LeftArrow, ConsoleKey.B);
      Assert.AreEqual(1, SingleLineEditor.CursorPosition);
    }

    [Test]
    public void Input_some_character_AND_left_MUST_move_the_cursor_back() {
      Input(ConsoleKey.A);
      Assert.AreEqual(1, SingleLineEditor.CursorPosition);
      Input(ConsoleKey.LeftArrow);
      Assert.AreEqual(0, SingleLineEditor.CursorPosition);
    }

    [Test]
    public void Going_right_at_the_end_of_line_MUST_not_move_the_cursor() {
      Input(ConsoleKey.A, ConsoleKey.RightArrow);
      Assert.AreEqual(1, SingleLineEditor.CursorPosition);
    }

    [Test]
    public void Going_left_and_right_MUST_move_the_cursor_to_the_same_position() {
      Input(ConsoleKey.A, ConsoleKey.LeftArrow, ConsoleKey.RightArrow);
      Assert.AreEqual(1, SingleLineEditor.CursorPosition);
    }

    [Test]
    public void Going_home_MUST_move_the_cursor_to_the_front() {
      Input(ConsoleKey.A, ConsoleKey.B, ConsoleKey.Home);
      Assert.AreEqual(0, SingleLineEditor.CursorPosition);
    }

    [Test]
    public void pressing_end_AFTER_some_character_input_and_going_left_MUST_move_the_cursor_to_the_end() {
      Input(ConsoleKey.A, ConsoleKey.B, ConsoleKey.LeftArrow, ConsoleKey.LeftArrow, ConsoleKey.End);
      Assert.AreEqual(2, ConsoleBuffer.CursorLeft);
    }

    [Test]
    public void Input_character_after_left_MUST_move_the_trailing_characters_to_right() {
      Input(ConsoleKey.A, ConsoleKey.LeftArrow, ConsoleKey.C);
      Assert.AreEqual(ToBuffer("CA"), ConsoleBuffer.ToArray());
    }

    [Test]
    public void Deleting_characters_in_the_middle_MUST_return_the_edited_line() {
      Input("This is madness.");
      Input(ConsoleKey.LeftArrow, 9);
      Input(ConsoleKey.Backspace, 2);
      Input("was");
      const string expectedLine = "This was madness.";
      Assert.AreEqual(expectedLine, SingleLineEditor.Line);
      Assert.AreEqual(ToBuffer(expectedLine), ConsoleBuffer.ToArray());
      Assert.AreEqual(8, SingleLineEditor.CursorPosition);
    }

    [Test]
    public void Deleting_characters_in_the_middle_MUST_move_the_buffer() {
      Input("012345");
      Input(ConsoleKey.LeftArrow, ConsoleKey.LeftArrow, ConsoleKey.Backspace);
      Assert.AreEqual(ToBuffer("01245"), ConsoleBuffer.ToArray());
      Assert.AreEqual(3, SingleLineEditor.CursorPosition);
    }

    [Test]
    public void Deleting_characters_at_the_end_MUST_clear_the_trailing_character() {
      Input(ConsoleKey.A, ConsoleKey.Backspace);
      Assert.AreEqual('\0', ConsoleBuffer[0, 0]);
    }

    [Test]
    public void Going_left_before_input_character_WHEN_non_zero_start_position_MUST_produce_a_single_character_line() {
      SetCursorPosition(5, 1);
      Input(ConsoleKey.LeftArrow, ConsoleKey.A);
      Assert.AreEqual("A", SingleLineEditor.Line);
      Assert.AreEqual(1, SingleLineEditor.CursorPosition);
    }

    [Test]
    public void Input_characters_WHEN_non_zero_start_position_MUST_produce_line_made_of_the_characters() {
      SetCursorPosition(5, 1);
      Input("foo");
      Assert.AreEqual("foo", SingleLineEditor.Line);
      Assert.AreEqual(3, SingleLineEditor.CursorPosition);
    }

    [Test]
    public void Deleting_a_character_WHEN_non_zero_start_position_MUST_produce_an_empty_line() {
      SetCursorPosition(5, 1);
      Input(ConsoleKey.A, ConsoleKey.Backspace);
      Assert.IsEmpty(SingleLineEditor.Line);
    }

    [Test]
    public void Inserting_a_character_WHEN_non_zero_start_position_MUST_push_the_buffer_correctly() {
      SetCursorPosition(5, 1);
      Input(ConsoleKey.A, ConsoleKey.LeftArrow, ConsoleKey.B);
      Assert.AreEqual(ToBuffer("", "\0\0\0\0\0BA"), ConsoleBuffer.ToArray());
    }

    [Test]
    public void Entering_a_line_that_matches_the_buffer_width_MUST_produce_the_correct_line() {
      var bufferWidthLine = TwoRowLine.Substring(0, ConsoleBuffer.BufferWidth);
      Input(bufferWidthLine);
      Assert.AreEqual(bufferWidthLine, SingleLineEditor.Line);
      Assert.AreEqual(ToBuffer(bufferWidthLine), ConsoleBuffer.ToArray());
      Assert.AreEqual(bufferWidthLine.Length, SingleLineEditor.CursorPosition);
    }

    [Test]
    public void Long_input_MUST_result_in_a_long_line() {
      Input(TwoRowLine);
      var expectedBuffer = ToBuffer(TwoRowLine.Substring(0, ConsoleBuffer.BufferWidth), TwoRowLine.Substring(ConsoleBuffer.BufferWidth));
      Assert.AreEqual(TwoRowLine.Length, SingleLineEditor.CursorPosition);
      Assert.AreEqual(TwoRowLine, SingleLineEditor.Line);
      Assert.AreEqual(expectedBuffer, ConsoleBuffer.ToArray());
    }

    private void SetCursorPosition(int cursorLeft, int cursorTop) {
      ConsoleBuffer = new MemoryConsoleBuffer(20, 3);
      ConsoleBuffer.CursorLeft = cursorLeft;
      ConsoleBuffer.CursorTop = cursorTop;
      SingleLineEditor = new SingleLineEditor(ConsoleBuffer);
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

    private void Input(ConsoleKey consoleKey, int repeatCount) => Input(ToInput(consoleKey), repeatCount);

    private void Input(ConsoleKey consoleKey) => Input(ToInput(consoleKey), 1);

    private void Input(string str) {
      foreach (var chr in str) {
        Input(new ConsoleKeyInfo(chr, (ConsoleKey) char.ToUpperInvariant(chr), false, false, false), 1);
      }
    }

    private static ConsoleKeyInfo ToInput(ConsoleKey consoleKey) {
      return new ConsoleKeyInfo((char) consoleKey, consoleKey, false, false, false);
    }

    private void VerifyConsoleBufferHasInitialState() {
      Assert.That(ConsoleBuffer.ToArray(), Is.All.EqualTo('\0'));
      Assert.AreEqual(0, ConsoleBuffer.CursorLeft);
      Assert.AreEqual(0, ConsoleBuffer.CursorTop);
    }

    private char[] ToBuffer(params string[] bufferRows) {
      var buffer = new char[ConsoleBuffer.BufferWidth * ConsoleBuffer.BufferHeight];
      for (int rowIndex = 0; rowIndex < bufferRows.Length; rowIndex++) {
        Array.Copy(bufferRows[rowIndex].ToCharArray(), 0, buffer, rowIndex * ConsoleBuffer.BufferWidth, bufferRows[rowIndex].Length);
      }
      return buffer;
    }
  }
}