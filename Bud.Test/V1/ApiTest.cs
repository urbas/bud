using System;
using Bud.Configuration;
using NUnit.Framework;
using static Bud.V1.Api;
using static NUnit.Framework.Assert;

namespace Bud.V1 {
  public class ApiTest {
    [Test]
    public void Nested_projects_must_inherit_BaseDir()
      => AreEqual("/foo",
                  Projects(Project("A")).Set(BaseDir, "/foo").Get("A"/BaseDir));

    [Test]
    public void ProjectId_cannot_be_null_or_empty()
      => That(Throws<ArgumentNullException>(() => Project(null)).Message,
              Does.Contain("A project's ID must not be null or empty."));

    [Test]
    public void ProjectId_cannot_contain_slashes()
      => That(Throws<ArgumentException>(() => Project("A/B")).Message,
              Does.Contain("Project ID 'A/B' is invalid. It must not contain the character '/'."));

    [Test]
    public void Project_throws_when_no_base_directory_is_given()
      => Throws<ConfAccessException>(() => Project("Foo").Get(BaseDir));

    [Test]
    public void BaseDir_is_set_when_given()
      => AreEqual("/bar",
                  Project("A", "/bar").Get(BaseDir));

    [Test]
    public void BaseDir_overrides_the_inherited_one()
      => AreEqual("/bar",
                  Projects(Project("A", "/bar"))
                    .Set(BaseDir, "/foo")
                    .Get("A"/BaseDir));
  }
}