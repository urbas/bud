using NUnit.Framework;

namespace Bud.Cli {
  public class ConsoleBufferUtilsTest {
    [Test]
    public void ShiftBufferRight_MUST_move_the_first_character_to_the_second_position() {
      MemoryConsoleBuffer buffer = new MemoryConsoleBuffer(2, 1, "a");
      buffer.ShiftBufferRight(0, 0, 1);
      Assert.AreEqual(ConsoleBufferTestUtils.ToCharBuffer(2, 1, "\0a"), buffer.CharBuffer);
    }
  }
}