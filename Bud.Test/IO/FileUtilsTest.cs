using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using static Bud.IO.FileUtils;
using static NUnit.Framework.Assert;

namespace Bud.IO {
  public class FileUtilsTest {
    [Test]
    public void ToTimestampedFile_returns_a_timestamped_file_with_the_right_path_and_timestamp() {
      using (var tmpDir = new TmpDir()) {
        var fileA = tmpDir.CreateEmptyFile();
        var timestampedFile = ToTimestampedFile(fileA);
        AreEqual(fileA, timestampedFile.Value);
        AreEqual(GetFileTimestamp(fileA), timestampedFile.Timestamp);
      }
    }

    [Test]
    public void GetFileTimestamp_return_the_last_write_time_of_the_file() {
      using (var tmpDir = new TmpDir()) {
        var fileA = tmpDir.CreateEmptyFile();
        var timestampedFile = ToTimestampedFile(fileA);
        var fileTimestampNow = FileTimestampNow();
        That(timestampedFile.Timestamp,
                    Is.GreaterThan(fileTimestampNow - 1000000)
                      .And.LessThan(fileTimestampNow + 1000000));
      }
    }

    [Test]
    public void IsNewerThan_returns_true_when_given_no_reference_files()
      => IsTrue(IsNewerThan("A", Enumerable.Empty<string>()));

    [Test]
    public void IsNewerThan_returns_false_when_a_reference_file_is_newer() {
      using (var tmpDir = new TmpDir()) {
        var fileA = tmpDir.CreateEmptyFile("A");
        File.SetLastWriteTime(fileA, DateTime.Now - TimeSpan.FromDays(2));
        var fileB = tmpDir.CreateEmptyFile("B");
        File.SetLastWriteTime(fileB, DateTime.Now - TimeSpan.FromDays(1));
        IsFalse(IsNewerThan(fileA, new[]{fileB}));
      }
    }

    [Test]
    public void IsNewerThan_returns_true_when_all_reference_files_are_older() {
      using (var tmpDir = new TmpDir()) {
        var fileB = tmpDir.CreateEmptyFile("B");
        File.SetLastWriteTime(fileB, DateTime.Now - TimeSpan.FromDays(3));
        var fileC = tmpDir.CreateEmptyFile("C");
        File.SetLastWriteTime(fileC, DateTime.Now - TimeSpan.FromDays(2));
        var fileA = tmpDir.CreateEmptyFile("A");
        File.SetLastWriteTime(fileA, DateTime.Now - TimeSpan.FromDays(1));
        IsTrue(IsNewerThan(fileA, new []{fileB, fileC}));
      }
    }
  }
}