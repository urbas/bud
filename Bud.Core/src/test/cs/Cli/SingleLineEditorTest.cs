using System;
using NUnit.Framework;

namespace Bud.Cli {
  public class SingleLineEditorTest {
    private const string TwoRowLine = "This is a two-row line. I am not lying.";
    private MemoryConsoleBuffer ConsoleBuffer;
    private SingleLineEditor SingleLineEditor;

    [SetUp]
    public void SetUp() {
      ConsoleBuffer = new MemoryConsoleBuffer(20, 5);
      SingleLineEditor = new SingleLineEditor(ConsoleBuffer);
    }

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
      Assert.AreEqual(ToBuffer("a"), ConsoleBuffer.CharBuffer);
    }

    [Test]
    public void Backspacing_the_single_character_MUST_produce_an_empty_line() {
      Input(ConsoleKey.A, ConsoleKey.Backspace);
      Assert.IsEmpty(SingleLineEditor.Line);
    }

    [Test]
    public void Backspacing_the_single_character_MUST_move_the_cursor_position_to_0() {
      Input(ConsoleKey.A, ConsoleKey.Backspace);
      Assert.AreEqual(0, SingleLineEditor.CursorPosition);
    }

    [Test]
    public void Backspacing_WHEN_line_is_empty_MUST_not_interact_with_console_buffer() {
      Input(ConsoleKey.Backspace);
      VerifyConsoleBufferHasInitialState();
    }

    [Test]
    public void Deleting_the_single_character_MUST_produce_an_empty_line() {
      Input(ConsoleKey.A, ConsoleKey.Home, ConsoleKey.Delete);
      Assert.IsEmpty(SingleLineEditor.Line);
    }

    [Test]
    public void Deleting_the_single_character_MUST_move_the_cursor_position_to_0() {
      Input(ConsoleKey.A, ConsoleKey.Home, ConsoleKey.Delete);
      Assert.AreEqual(0, SingleLineEditor.CursorPosition);
    }

    [Test]
    public void Deleting_WHEN_at_the_end_of_line_MUST_not_change_the_buffer() {
      Input(ConsoleKey.A, ConsoleKey.Delete);
      Assert.AreEqual(ToBuffer("A"), ConsoleBuffer.CharBuffer);
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
    public void Going_home_MUST_move_the_cursor_to_the_front_WHEN_in_a_multi_row_scenario() {
      SetCursorPosition(5, 1);
      Input(TwoRowLine);
      Input(ConsoleKey.Home);
      Assert.AreEqual(0, SingleLineEditor.CursorPosition);
      Assert.AreEqual(5, ConsoleBuffer.CursorLeft);
      Assert.AreEqual(1, ConsoleBuffer.CursorTop);
    }

    [Test]
    public void Pressing_end_MUST_move_the_cursor_to_the_back_WHEN_in_a_multi_row_scenario() {
      SetCursorPosition(5, 1);
      Input(TwoRowLine);
      Input(ConsoleKey.Home, ConsoleKey.End);
      Assert.AreEqual(TwoRowLine.Length, SingleLineEditor.CursorPosition);
      Assert.AreEqual(4, ConsoleBuffer.CursorLeft);
      Assert.AreEqual(3, ConsoleBuffer.CursorTop);
    }

    [Test]
    public void Pressing_end_WHEN_line_ends_just_before_the_end_of_the_row_MUST_place_the_cursor_at_the_end_of_that_row() {
      Input(TwoRowLine);
      Input(ConsoleKey.Home, ConsoleKey.End, ConsoleKey.A);
      Assert.AreEqual(0, ConsoleBuffer.CursorLeft);
      Assert.AreEqual(2, ConsoleBuffer.CursorTop);
    }

    [Test]
    public void Pressing_end_AFTER_some_character_input_and_going_left_MUST_move_the_cursor_to_the_end() {
      Input(ConsoleKey.A, ConsoleKey.B, ConsoleKey.LeftArrow, ConsoleKey.LeftArrow, ConsoleKey.End);
      Assert.AreEqual(2, ConsoleBuffer.CursorLeft);
    }

    [Test]
    public void Input_character_after_left_MUST_move_the_trailing_characters_to_right() {
      Input(ConsoleKey.A, ConsoleKey.LeftArrow, ConsoleKey.C);
      Assert.AreEqual(ToBuffer("CA"), ConsoleBuffer.CharBuffer);
    }

    [Test]
    public void Backspacing_characters_in_the_middle_MUST_return_the_edited_line() {
      Input("This is madness.");
      Input(ConsoleKey.LeftArrow, 9);
      Input(ConsoleKey.Backspace, 2);
      Input("was");
      const string expectedLine = "This was madness.";
      Assert.AreEqual(expectedLine, SingleLineEditor.Line);
      Assert.AreEqual(ToBuffer(expectedLine), ConsoleBuffer.CharBuffer);
      Assert.AreEqual(8, SingleLineEditor.CursorPosition);
    }

    [Test]
    public void Backspacing_characters_in_the_middle_MUST_move_the_buffer() {
      Input("012345");
      Input(ConsoleKey.LeftArrow, ConsoleKey.LeftArrow, ConsoleKey.Backspace);
      Assert.AreEqual(ToBuffer("01245"), ConsoleBuffer.CharBuffer);
      Assert.AreEqual(3, SingleLineEditor.CursorPosition);
    }

    [Test]
    public void Backspacing_characters_at_the_end_MUST_clear_the_trailing_character() {
      Input(ConsoleKey.A, ConsoleKey.Backspace);
      VerifyConsoleBufferHasInitialState();
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
    public void Backspacing_a_character_WHEN_non_zero_start_position_MUST_produce_an_empty_line() {
      SetCursorPosition(5, 1);
      Input(ConsoleKey.A, ConsoleKey.Backspace);
      Assert.IsEmpty(SingleLineEditor.Line);
    }

    [Test]
    public void Changing_the_width_of_the_buffer_MUST_not_affect_the_cursor_position() {
      Input("0123456789012345");
      SetBufferWidth(10);
      Assert.AreEqual(16, SingleLineEditor.CursorPosition);
    }

    [Test]
    public void Changing_the_width_of_the_buffer_MUST_recalculate_layout_on_next_input() {
      Input("0123456789012345");
      SetBufferWidth(10);
      Input(ConsoleKey.A);
      Assert.AreEqual(1, ConsoleBuffer.CursorTop);
      Assert.AreEqual(7, ConsoleBuffer.CursorLeft);
      Assert.AreEqual(ToBuffer("0123456789", "012345A"), ConsoleBuffer.CharBuffer);
    }

    [Test]
    public void Cursor_start_left_position_MUST_be_reset_to_zero_WHEN_the_width_of_the_buffer_shrinks_below_cursor_start_position() {
      SetCursorPosition(12, 0);
      SetBufferWidth(10);
      Input(ConsoleKey.A);
      Assert.AreEqual(0, ConsoleBuffer.CursorTop);
      Assert.AreEqual(1, ConsoleBuffer.CursorLeft);
      Assert.AreEqual(ToBuffer("A"), ConsoleBuffer.CharBuffer);
    }

    [Test]
    public void Inserting_a_character_WHEN_non_zero_start_position_MUST_push_the_buffer_correctly() {
      SetCursorPosition(5, 1);
      Input(ConsoleKey.A, ConsoleKey.LeftArrow, ConsoleKey.B);
      Assert.AreEqual(ToBuffer("", "\0\0\0\0\0BA"), ConsoleBuffer.CharBuffer);
    }

    [Test]
    public void Entering_a_line_that_matches_the_buffer_width_MUST_produce_the_correct_line() {
      var bufferWidthLine = TwoRowLine.Substring(0, ConsoleBuffer.BufferWidth);
      Input(bufferWidthLine);
      Assert.AreEqual(bufferWidthLine, SingleLineEditor.Line);
      Assert.AreEqual(ToBuffer(bufferWidthLine), ConsoleBuffer.CharBuffer);
      Assert.AreEqual(bufferWidthLine.Length, SingleLineEditor.CursorPosition);
    }

    [Test]
    public void Long_input_MUST_result_in_a_long_line() {
      Input(TwoRowLine);
      var expectedBuffer = ToBuffer(TwoRowLine.Substring(0, ConsoleBuffer.BufferWidth), TwoRowLine.Substring(ConsoleBuffer.BufferWidth));
      Assert.AreEqual(TwoRowLine.Length, SingleLineEditor.CursorPosition);
      Assert.AreEqual(TwoRowLine, SingleLineEditor.Line);
      Assert.AreEqual(expectedBuffer, ConsoleBuffer.CharBuffer);
    }

    [Test]
    public void Going_left_to_the_start_and_back_again_MUST_move_the_cursor_to_the_end() {
      Input(TwoRowLine);
      var cursorLeft = ConsoleBuffer.CursorLeft;
      var cursorTop = ConsoleBuffer.CursorTop;
      Input(ConsoleKey.LeftArrow, TwoRowLine.Length);
      Assert.AreEqual(0, ConsoleBuffer.CursorLeft);
      Assert.AreEqual(0, ConsoleBuffer.CursorTop);
      Input(ConsoleKey.RightArrow, TwoRowLine.Length);
      Assert.AreEqual(cursorLeft, ConsoleBuffer.CursorLeft);
      Assert.AreEqual(cursorTop, ConsoleBuffer.CursorTop);
    }

    [Test]
    public void Backspacing_a_character_at_the_start_of_the_second_row_MUST_move_the_cursor_up_to_the_first_row() {
      Input(TwoRowLine.Substring(0, ConsoleBuffer.BufferWidth));
      Input(ConsoleKey.Backspace);
      Assert.AreEqual(0, ConsoleBuffer.CursorTop);
      Assert.AreEqual(ConsoleBuffer.BufferWidth - 1, ConsoleBuffer.CursorLeft);
    }

    [Test]
    public void Resetting_the_origin_MUST_copy_the_line_to_the_new_place_in_the_buffer() {
      Input("foo bar");
      ConsoleBuffer.CursorLeft = ConsoleBuffer.BufferWidth - 1;
      ConsoleBuffer.CursorTop = 1;
      SingleLineEditor.ResetOrigin();
      Assert.AreEqual(6, ConsoleBuffer.CursorLeft);
      Assert.AreEqual(2, ConsoleBuffer.CursorTop);
    }

    [Test]
    public void Inserting_text_MUST_change_the_line_contents() {
      SingleLineEditor.InsertText("foo");
      Assert.AreEqual("foo", SingleLineEditor.Line);
    }

    [Test]
    public void Inserting_text_MUST_change_the_cursor_position() {
      SingleLineEditor.InsertText("foo");
      Assert.AreEqual(3, SingleLineEditor.CursorPosition);
    }

    [Test]
    public void Inserting_text_MUST_add_the_text_into_the_buffer() {
      SingleLineEditor.InsertText("foo");
      Assert.AreEqual(ToBuffer("foo"), ConsoleBuffer.CharBuffer);
      Assert.AreEqual(3, ConsoleBuffer.CursorLeft);
    }

    private void SetCursorPosition(int cursorLeft, int cursorTop) {
      ConsoleBuffer.CursorLeft = cursorLeft;
      ConsoleBuffer.CursorTop = cursorTop;
      SingleLineEditor.ResetOrigin();
    }

    private void SetBufferWidth(int newBufferWidth) {
      ConsoleBuffer.BufferWidth = newBufferWidth;
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

    private void Input(ConsoleKey consoleKey, int repeatCount) => Input(ConsoleBufferTestUtils.ToInput(consoleKey), repeatCount);

    private void Input(ConsoleKey consoleKey) => Input(ConsoleBufferTestUtils.ToInput(consoleKey), 1);

    private void Input(string str) {
      foreach (var chr in str) {
        Input(new ConsoleKeyInfo(chr, ToConsoleKey(chr), false, false, false), 1);
      }
    }

    private static ConsoleKey ToConsoleKey(char chr) {
      return chr == '.' ? ConsoleKey.OemPeriod : (ConsoleKey) char.ToUpperInvariant(chr);
    }

    private void VerifyConsoleBufferHasInitialState() {
      Assert.That(ConsoleBuffer.CharBuffer, Is.All.EqualTo('\0'));
      Assert.AreEqual(0, ConsoleBuffer.CursorLeft);
      Assert.AreEqual(0, ConsoleBuffer.CursorTop);
    }

    private char[] ToBuffer(params string[] bufferRows) => ConsoleBufferTestUtils.ToCharBuffer(ConsoleBuffer.BufferWidth, ConsoleBuffer.BufferHeight, bufferRows);
  }
}