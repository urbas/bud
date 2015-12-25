using NUnit.Framework;
using static Bud.IO.FileUtils;

namespace Bud.IO {
  public class FileUtilsTest {
    [Test]
    public void ToTimestampedFile_returns_a_timestamped_file_with_the_right_path_and_timestamp() {
      using (var tmpDir = new TemporaryDirectory()) {
        var fileA = tmpDir.CreateEmptyFile("A");
        var timestampedFile = ToTimestampedFile(fileA);
        Assert.AreEqual(fileA, timestampedFile.Value);
        Assert.AreEqual(GetFileTimestamp(fileA), timestampedFile.Timestamp);
      }
    }

    [Test]
    public void GetFileTimestamp_return_the_last_write_time_of_the_file() {
      using (var tmpDir = new TemporaryDirectory()) {
        var fileA = tmpDir.CreateEmptyFile("A");
        var timestampedFile = ToTimestampedFile(fileA);
        var fileTimestampNow = FileTimestampNow();
        Assert.That(timestampedFile.Timestamp,
                    Is.GreaterThan(fileTimestampNow - 1000000)
                      .And.LessThan(fileTimestampNow + 1000000));
      }
    }
  }
}