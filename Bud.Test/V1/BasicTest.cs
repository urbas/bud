using System;
using System.IO;
using Bud.Configuration;
using NUnit.Framework;
using static Bud.V1.Basic;
using static NUnit.Framework.Assert;

namespace Bud.V1 {
  public class BasicTest {
    [Test]
    public void Nested_projects_must_inherit_BaseDir()
      => AreEqual("/foo",
                  Projects(BareProject("A")).Set(BaseDir, "/foo").Get("A"/BaseDir));

    [Test]
    public void ProjectId_cannot_be_null_or_empty()
      => That(Throws<ArgumentNullException>(() => BareProject(null)).Message,
              Does.Contain("A project's ID must not be null or empty."));

    [Test]
    public void ProjectId_cannot_contain_slashes()
      => That(Throws<ArgumentException>(() => BareProject("A/B")).Message,
              Does.Contain("Project ID 'A/B' is invalid. It must not contain the character '/'."));

    [Test]
    public void Project_throws_when_no_base_directory_is_given()
      => Throws<ConfAccessException>(() => BareProject("Foo").Get(BaseDir));

    [Test]
    public void BaseDir_is_set_when_given()
      => AreEqual("/bar",
                  BareProject("A", "/bar").Get(BaseDir));

    [Test]
    public void BaseDir_overrides_the_inherited_one()
      => AreEqual("/bar",
                  Projects(BareProject("A", "/bar"))
                    .Set(BaseDir, "/foo")
                    .Get("A"/BaseDir));

    [Test]
    public void Set_ProjectId()
      => AreEqual("A", BareProject("A", "/foo").Get(ProjectId));

    [Test]
    public void Set_ProjectDir()
      => AreEqual(Path.Combine("/foo", "bar"),
                  Project("A", "bar", "/foo").Get(ProjectDir));

    [Test]
    public void Set_ProjectDir_from_ProjectId()
      => AreEqual(Path.Combine("/foo", "A"),
                  Project("A", baseDir: "/foo").Get(ProjectDir));

    [Test]
    public void ProjectDir_must_be_relative_to_BaseDir()
      => AreEqual(Path.Combine("/bar", "foo"),
                  Project("A", "foo", "/bar").Get(ProjectDir));

    [Test]
    public void ProjectDir_must_be_unchanged_if_absolute()
      => AreEqual("/bar",
                  Project("A", "/bar", "/foo").Get(ProjectDir));

    [Test]
    public void ProjectDir_is_BaseDir_when_empty()
      => AreEqual("/foo",
                  Project("A", "", "/foo").Get(ProjectDir));

    [Test]
    public void BaseDir_is_not_set_by_default()
      => Throws<ConfAccessException>(() => Project("A").Get(BaseDir));

    [Test]
    public void BuildDir_is_within_BaseDir()
      => AreEqual(Path.Combine("/foo", "build", "A"),
                  Project("A", baseDir: "/foo").Get(BuildDir));

    [Test]
    public void ProjectVersion_is_initially_default()
      => AreEqual("0.0.1",
                  Project("A", "", "/foo").Get(ProjectVersion));

    [Test]
    public void Dependencies_should_be_initially_empty()
      => IsEmpty(Project("A").Get(Dependencies));
  }
}