using System;
using System.IO;
using NUnit.Framework;

namespace Bud.TempDir {
  public class TemporaryDirectoryTest {
    [Test]
    public void Creates_the_directory() {
      using (var dir = new TemporaryDirectory()) {
        Assert.That(dir.Path, Does.Exist);
      }
    }

    [Test]
    public void Deletes_the_directory() {
      string path;
      using (var dir = new TemporaryDirectory()) {
        path = dir.Path;
      }
      Assert.That(path, Does.Not.Exist);
    }

    [Test]
    public void Lists_locked_files_when_failing_to_delete_directory() {
      var dir = new TemporaryDirectory();
      var fileA = dir.CreateEmptyFile("a");
      using (var _ = File.OpenRead(fileA)) {
        var ex = Assert.Throws<Exception>(() => dir.Dispose());
        Assert.That(ex.Message,
          Contains.Substring(fileA));
      }
      dir.Dispose();
      Assert.That(dir.Path, Does.Not.Exist);
    }

    [Test]
    public void ToString_returns_the_path() {
      using (var dir = new TemporaryDirectory()) {
        Assert.AreEqual(dir.Path, dir.ToString());
      }
    }

    [Test]
    public void CreateDir_creates_a_subdirectory() {
      using (var dir = new TemporaryDirectory()) {
        dir.CreateDir("a");
        DirectoryAssert.Exists(Path.Combine(dir.Path, "a"));
      }
    }

    [Test]
    public void CreateFileFromResource_produces_the_file() {
      using (var dir = new TemporaryDirectory()) {
        var fileFromResource = dir.CreateFileFromResource("Bud.TempDir.FooEmbeddedResource", "a");
        var expectedContent = dir.CreateFile("foo", "b");
        FileAssert.AreEqual(expectedContent, fileFromResource);
      }
    }
  }
}