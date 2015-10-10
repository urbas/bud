using System;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
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
    [Ignore("This test uses Thread.Sleep.")]
    public async void Adding_a_file_should_result_in_a_push_notification() {
      var file = Task.Run(() => {
        Thread.Sleep(TimeSpan.FromMilliseconds(100));
        return tempDir.CreateEmptyFile("A", "A.txt");
      });

      var fileSystemEventArgs = await LocalFilesObservatory.ObserveFileSystem(tempDir.Path, "*.txt", true).Take(1).ToTask();

      Assert.AreEqual(WatcherChangeTypes.Created, fileSystemEventArgs.ChangeType);
      Assert.AreEqual(await file, fileSystemEventArgs.FullPath);
    }
  }
}