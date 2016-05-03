using Bud.Configuration;
using Bud.IO;
using Bud.V1;
using NUnit.Framework;
using static NUnit.Framework.Assert;
using static System.IO.Path;
using static Bud.V1.Api;

namespace Bud.BaseProjects {
  public class BareProjectsTest {
    [Test]
    public void BaseDir_is_not_set_by_default()
      => Throws<ConfAccessException>(() => BareProject("A").Get(BaseDir));

    [Test]
    public void Set_ProjectDir()
      => AreEqual(Combine("/foo", "bar"),
                  ProjectDir[BareProject("bar", "Foo").Set(BaseDir, "/foo")]);

    [Test]
    public void Set_ProjectDir_from_ProjectId()
      => AreEqual(Combine("/foo", "Foo"),
                  ProjectDir[BareProject("Foo").Set(BaseDir, "/foo")]);

    [Test]
    public void Set_ProjectId()
      => AreEqual("Foo", ProjectId[BareProject("bar", "Foo")]);

    [Test]
    public void BuildDir_is_within_the_base_build_directory()
      => AreEqual(Combine("/foo/bar", "build", "A"),
                  BareProject("A").Set(BaseDir, "/foo/bar").Get(BuildDir));

    [Test]
    public void Dependencies_should_be_initially_empty()
      => IsEmpty(Dependencies[BuildProject("bar", "Foo")]);

    [Test]
    public void ProjectDir_must_be_relative_to_BaseDir() {
      using (var tmpDir = new TemporaryDirectory()) {
        AreEqual(Combine(tmpDir.Path, "foo"),
                 BareProject("foo", "Foo")
                   .Set(BaseDir, tmpDir.Path)
                   .Get(ProjectDir));
      }
    }

    [Test]
    public void ProjectDir_must_be_unchanged_if_absolute() {
      using (var tmpDir = new TemporaryDirectory()) {
        AreEqual(tmpDir.Path,
                 BareProject(tmpDir.Path, "Foo")
                   .Set(BaseDir, "/foo")
                   .Get(ProjectDir));
      }
    }

    [Test]
    public void BaseDir_must_be_taken_from_level_above() {
      using (var tmpDir = new TemporaryDirectory()) {
        AreEqual(Combine(tmpDir.Path, "foo"),
                 Projects(BareProject("foo", "Foo"))
                   .Set(BaseDir, tmpDir.Path)
                   .Get("Foo"/ProjectDir));
      }
    }

    [Test]
    public void ProjectDir_is_BaseDir_when_empty()
      => AreEqual("/foo",
                  ProjectDir[BareProject("", "Foo").Set(BaseDir, "/foo")]);

    [Test]
    public void ProjectDir_equals_to_BaseDir_when_empty() {
      using (var tmpDir = new TemporaryDirectory()) {
        AreEqual(tmpDir.Path,
                 BareProject("", "Foo")
                   .Set(BaseDir, tmpDir.Path)
                   .Get(ProjectDir));
      }
    }
  }
}