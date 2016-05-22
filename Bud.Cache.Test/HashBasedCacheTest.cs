using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Bud.TempDir;
using Moq;
using NUnit.Framework;

namespace Bud.Cache.Test {
  public class HashBasedCacheTest {
    [Test]
    public void Get_invokes_the_file_generator_when_nothing_in_cache() {
      using (var dir = new TemporaryDirectory()) {
        var cache = new HashBasedCache(dir.Path);
        var contentProducer = new Mock<Action<string>>();
        cache.Get(new byte[] {0x13}, contentProducer.Object);
        contentProducer.Verify(self => self(It.IsAny<string>()), Times.Once);
      }
    }

    [Test]
    public void Get_does_not_invoke_the_file_generator_when_already_in_cache() {
      using (var dir = new TemporaryDirectory()) {
        var hash = new byte[] {0x13};
        dir.CreateDir(HashBasedCache.ByteToHexString(hash));
        var cache = new HashBasedCache(dir.Path);
        var contentProducer = new Mock<Action<string>>();
        cache.Get(hash, contentProducer.Object);
        contentProducer.Verify(self => self(It.IsAny<string>()), Times.Never);
      }
    }

    [Test]
    public void Get_invokes_producer_only_once() {
      using (var dir = new TemporaryDirectory()) {
        var hash = new byte[] {0x13};
        var cache = new HashBasedCache(dir.Path);
        var contentProducer = new Mock<Action<string>>();
        cache.Get(hash, contentProducer.Object);
        cache.Get(hash, contentProducer.Object);
        contentProducer.Verify(self => self(It.IsAny<string>()), Times.Once);
      }
    }

    [Test]
    public void Get_does_not_create_cache_entry_if_producer_fails() {
      using (var dir = new TemporaryDirectory()) {
        var hash = new byte[] {0x13};
        var cache = new HashBasedCache(dir.Path);
        try {
          cache.Get(hash, s => {
            throw new Exception();
          });
        } catch (Exception) {
          // ignored
        }
        var contentProducer = new Mock<Action<string>>();
        cache.Get(hash, contentProducer.Object);
        contentProducer.Verify(self => self(It.IsAny<string>()), Times.Once);
      }
    }

    [Test]
    public void Get_does_not_leave_temporary_directories_behind_on_failure() {
      using (var dir = new TemporaryDirectory()) {
        var hash = new byte[] {0x13};
        var cache = new HashBasedCache(dir.Path);
        try {
          cache.Get(hash, s => {
            throw new Exception();
          });
        } catch (Exception) {
          // ignored
        }
        Assert.IsEmpty(Directory.EnumerateDirectories(dir.Path, "*", SearchOption.TopDirectoryOnly));
      }
    }

    [Test]
    public void Get_keeps_first_directory_when_two_were_created_in_parallel() {
      using (var dir = new TemporaryDirectory()) {
        var hash = new byte[] {0x13};
        var cache = new HashBasedCache(dir.Path);
        var firstStart = new CountdownEvent(1);
        var secondStart = new CountdownEvent(1);
        var firstFinished = new CountdownEvent(1);
        var secondFinished = new CountdownEvent(1);
        Task.Run(() => {
          cache.Get(hash, path => {
            firstStart.Signal();
            File.WriteAllText(Path.Combine(path, "foo"), "first");
            secondStart.Wait();
          });
          firstFinished.Signal();
        });
        firstStart.Wait();
        Task.Run(() => {
          cache.Get(hash, path => {
            secondStart.Signal();
            File.WriteAllText(Path.Combine(path, "foo"), "second");
            firstFinished.Wait();
          });
          secondFinished.Signal();
        });
        secondFinished.Wait();
        FileAssert.AreEqual(dir.CreateFile("first", "expected_content"),
                            dir.CreatePath(HashBasedCache.ByteToHexString(hash), "foo"));
      }
    }
  }
}