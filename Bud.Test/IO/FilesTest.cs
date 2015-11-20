using System.Linq;
using System.Reactive.Linq;
using NUnit.Framework;

namespace Bud.IO {
  public class FilesTest {
    [Test]
    public void Files_equals_to_the_initialising_enumeration() {
      var files = new[] {"foo", "bar"};
      Assert.AreEqual(files, new Files(files).Lister);
    }

    [Test]
    public void Expanded_with_empty_files_produces_same_files() {
      var files1 = new[] {"foo"};
      var files2 = Enumerable.Empty<string>();
      Assert.AreEqual(files1,
                      new Files(files1).ExpandWith(new Files(files2)).Lister);
    }

    [Test]
    public void Expanded_with_some_files_produces_a_concatenated_enumeration() {
      var files1 = new[] {"foo"};
      var files2 = new[] {"bar"};
      Assert.AreEqual(files1.Concat(files2),
                      new Files(files1).ExpandWith(new Files(files2)).Lister);
    }

    [Test]
    public void Watching_empty_produces_one_observation() {
      var emptyFilesWatcher = Files.Empty.Watch().GetEnumerator();
      Assert.IsTrue(emptyFilesWatcher.MoveNext());
      Assert.IsEmpty(emptyFilesWatcher.Current);
    }

    [Test]
    public void Expanding_and_watching_empty_produces_one_observation() {
      var files = new[] {"foo"};
      var expandedFilesWatcher = Files.Empty.ExpandWith(new Files(files)).Watch().GetEnumerator();
      Assert.IsTrue(expandedFilesWatcher.MoveNext());
      Assert.That(expandedFilesWatcher.Current, Is.EquivalentTo(files));
    }

    [Test]
    public void ToTimestampedFile_returns_a_timestamped_file_with_the_right_path_and_timestamp() {
      using (var tmpDir = new TemporaryDirectory()) {
        var fileA = tmpDir.CreateEmptyFile("A");
        var timestampedFile = Files.ToTimestampedFile(fileA);
        Assert.AreEqual(fileA, timestampedFile.Value);
        Assert.AreEqual(Files.GetFileTimestamp(fileA), timestampedFile.Timestamp);
      }
    }

    [Test]
    public void GetFileTimestamp_return_the_last_write_time_of_the_file() {
      using (var tmpDir = new TemporaryDirectory()) {
        var fileA = tmpDir.CreateEmptyFile("A");
        var timestampedFile = Files.ToTimestampedFile(fileA);
        var fileTimestampNow = Files.FileTimestampNow();
        Assert.That(timestampedFile.Timestamp,
                    Is.GreaterThan(fileTimestampNow - 1000000)
                      .And.LessThan(fileTimestampNow + 1000000));
      }
    }
  }
}