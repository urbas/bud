using NUnit.Framework;

namespace Bud.Cli {
  public class ConsoleBufferUtilsTest {
    [Test]
    public void ShiftBufferRight_MUST_move_the_first_character_to_the_second_column() {
      var buffer = new MemoryConsoleBuffer(2, 1, "a");
      buffer.ShiftBufferRight(0, 0, 1);
      Assert.AreEqual(ConsoleBufferTestUtils.ToCharBuffer(2, 1, "\0a"), buffer.CharBuffer);
    }

    [Test]
    public void ShiftBufferRight_MUST_move_the_first_character_to_the_second_row() {
      var buffer = new MemoryConsoleBuffer(1, 2, "a");
      buffer.ShiftBufferRight(0, 0, 1);
      Assert.AreEqual(ConsoleBufferTestUtils.ToCharBuffer(1, 2, "", "a"), buffer.CharBuffer);
    }

    [Test]
    public void ShiftBufferRight_MUST_move_the_overflowing_character_to_the_next_row() {
      var buffer = new MemoryConsoleBuffer(2, 2, "ab");
      buffer.ShiftBufferRight(0, 0, 2);
      Assert.AreEqual(ConsoleBufferTestUtils.ToCharBuffer(2, 2, "\0a", "b"), buffer.CharBuffer);
    }

    [Test]
    public void ShiftBufferRight_MUST_move_multiple_overflowing_characters_to_the_next_rows() {
      var buffer = new MemoryConsoleBuffer(1, 3, "a", "b");
      buffer.ShiftBufferRight(0, 0, 2);
      Assert.AreEqual(ConsoleBufferTestUtils.ToCharBuffer(1, 3, "", "a", "b"), buffer.CharBuffer);
    }

    [Test]
    public void ShiftBufferRight_MUST_must_shift_when_last_line_is_not_filled() {
      var buffer = new MemoryConsoleBuffer(2, 3, "ab", "cd");
      buffer.ShiftBufferRight(0, 0, 3);
      Assert.AreEqual(ConsoleBufferTestUtils.ToCharBuffer(2, 3, "\0a", "bc"), buffer.CharBuffer);
    }
  }
}