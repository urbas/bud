using System;
using System.IO;
using NUnit.Framework;

namespace Bud {
  public class TmpDirTest {
    [Test]
    public void Creates_the_directory() {
      using (var dir = new TmpDir()) {
        DirectoryAssert.Exists(dir.Path);
      }
    }

    [Test]
    public void Path_is_absolute() {
      using (var dir = new TmpDir()) {
        Assert.That(Path.IsPathRooted(dir.Path));
      }
    }

    [Test]
    public void Deletes_the_directory() {
      string path;
      using (var dir = new TmpDir()) {
        path = dir.Path;
      }
      Assert.That(path, Does.Not.Exist);
    }

    [Test]
    public void Exception_message_lists_locked_files_when_failing_to_delete_directory() {
      var dir = new TmpDir();
      var fileA = dir.CreateEmptyFile("a");
      var fileB = dir.CreateEmptyFile("b");
      using (var _ = File.OpenRead(fileA)) {
        var ex = Assert.Throws<Exception>(() => dir.Dispose());
        Assert.That(ex.Message,
                    Contains.Substring(fileA)
                            .And
                            .Not.Contains(fileB));
      }
      dir.Dispose();
      Assert.That(dir.Path, Does.Not.Exist);
    }

    [Test]
    public void ToString_returns_the_path() {
      using (var dir = new TmpDir()) {
        Assert.AreEqual(dir.Path, dir.ToString());
      }
    }

    [Test]
    public void CreateDir_creates_a_subdirectory() {
      using (var dir = new TmpDir()) {
        dir.CreateDir("a");
        DirectoryAssert.Exists(Path.Combine(dir.Path, "a"));
      }
    }

    [Test]
    public void CreateFileFromResource_produces_the_file() {
      using (var dir = new TmpDir()) {
        var fileFromResource = dir.CreateFileFromResource("Bud.FooEmbeddedResource", "a");
        var expectedContent = dir.CreateFile("foo", "b");
        FileAssert.AreEqual(expectedContent, fileFromResource);
      }
    }
  }
}