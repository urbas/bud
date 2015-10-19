using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Bud.IO {
  public class LocalFilesObservatoryTest {
    private TemporaryDirectory tempDir;

    [SetUp]
    public void SetUp() => tempDir = new TemporaryDirectory();

    [TearDown]
    public void TearDown() => tempDir.Dispose();

    [Test]
    public async void Adding_a_file_should_result_in_a_push_notification() {
      var changedFile = LocalFilesObservatory.ObserveFileSystem(tempDir.Path, "*.txt", true).Take(1).ToTask();
      var file = Task.Run(() => tempDir.CreateEmptyFile("A", "A.txt"));
      Assert.AreEqual(await file, await changedFile);
    }

    [Test]
    public async void Changing_a_file_should_result_in_a_push_notification() {
      var fileA = tempDir.CreateEmptyFile("A", "A.txt");
      var changedFile = LocalFilesObservatory.ObserveFileSystem(tempDir.Path, "*.txt", true).Take(1).ToTask();
      await Task.Run(() => File.WriteAllText(fileA, "foo"));
      Assert.AreEqual(fileA, await changedFile);
    }

    [Test]
    public async void Removing_a_file_should_result_in_a_push_notification() {
      var fileA = tempDir.CreateEmptyFile("A", "A.txt");
      var changedFile = LocalFilesObservatory.ObserveFileSystem(tempDir.Path, "*.txt", true).Take(1).ToTask();
      await Task.Run(() => File.Delete(fileA));
      Assert.AreEqual(fileA, await changedFile);
    }

    [Test]
    public async void Moving_a_file_should_result_in_two_push_notifications() {
      var fileA = tempDir.CreateEmptyFile("A", "A.txt");
      var fileB = Path.Combine(tempDir.Path, "B.txt");
      var changes = LocalFilesObservatory.ObserveFileSystem(tempDir.Path, "*.txt", true).GetEnumerator();
      await Task.Run(() => File.Move(fileA, fileB));
      changes.MoveNext();
      Assert.AreEqual(fileA, changes.Current);
      changes.MoveNext();
      Assert.AreEqual(fileB, changes.Current);
    }

    [Test]
    public async void Renaming_a_file_should_result_in_two_push_notifications() {
      var fileA = tempDir.CreateEmptyFile("A.txt");
      var fileB = Path.Combine(tempDir.Path, "B.txt");
      var changes = LocalFilesObservatory.ObserveFileSystem(tempDir.Path, "*.txt", true).GetEnumerator();
      await Task.Run(() => File.Move(fileA, fileB));
      changes.MoveNext();
      Assert.AreEqual(fileA, changes.Current);
      changes.MoveNext();
      Assert.AreEqual(fileB, changes.Current);
    }
  }
}