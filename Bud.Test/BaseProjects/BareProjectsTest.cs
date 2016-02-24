using Bud.IO;
using Bud.V1;
using NUnit.Framework;
using static System.IO.Directory;
using static NUnit.Framework.Assert;
using static System.IO.Path;
using static Bud.V1.Api;

namespace Bud.BaseProjects {
  public class BareProjectsTest {
    [Test]
    public void Set_the_projectDir()
      => AreEqual(Combine(GetCurrentDirectory(), "bar"),
                  ProjectDir[BareProject("bar", "Foo")]);

    [Test]
    public void Set_the_projectId()
      => AreEqual("Foo", ProjectId[BareProject("bar", "Foo")]);

    [Test]
    public void Target_directory_is_within_the_project_directory() {
      var project = BuildProject("fooDir", "foo");
      AreEqual(Combine(ProjectDir[project], "build"), BuildDir[project]);
    }

    [Test]
    public void Dependencies_should_be_initially_empty()
      => IsEmpty(Dependencies[BuildProject("bar", "Foo")]);

    [Test]
    public void ProjectDir_must_be_relative_to_BaseDir() {
      using (var tmpDir = new TemporaryDirectory()) {
        AreEqual(Combine(tmpDir.Path, "foo"),
                 BareProject("foo", "Foo")
                   .InitValue(BaseDir, tmpDir.Path)
                   .Get(ProjectDir));
      }
    }

    [Test]
    public void ProjectDir_must_be_unchanged_if_absolute() {
      using (var tmpDir = new TemporaryDirectory()) {
        AreEqual(tmpDir.Path,
                 BareProject(tmpDir.Path, "Foo")
                   .Get(ProjectDir));
      }
    }

    [Test]
    public void ProjectDir_must_be_unchanged_if_absolute_and_BaseDir_set() {
      using (var tmpDir = new TemporaryDirectory()) {
        AreEqual(tmpDir.Path,
                 BareProject(tmpDir.Path, "Foo")
                   .InitValue(BaseDir, tmpDir.Path)
                   .Get(ProjectDir));
      }
    }

    [Test]
    public void BaseDir_must_be_taken_from_level_above() {
      using (var tmpDir = new TemporaryDirectory()) {
        AreEqual(Combine(tmpDir.Path, "foo"),
                 Projects(BareProject("foo", "Foo"))
                   .InitValue(BaseDir, tmpDir.Path)
                   .Get("Foo"/ProjectDir));
      }
    }

    [Test]
    public void ProjectDir_is_current_directory_when_empty()
      => AreEqual(GetCurrentDirectory(),
                  ProjectDir[BareProject("", "Foo")]);

    [Test]
    public void ProjectDir_equals_to_BaseDir_when_empty() {
      using (var tmpDir = new TemporaryDirectory()) {
        AreEqual(tmpDir.Path,
                 BareProject("", "Foo")
                   .InitValue(BaseDir, tmpDir.Path)
                   .Get(ProjectDir));
      }
    }
  }
}