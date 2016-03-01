using System;
using NUnit.Framework;
using static Bud.V1.Api;
using static NUnit.Framework.Assert;

namespace Bud.V1 {
  public class ApiTest {
    [Test]
    public void Nested_projects_must_inherit_BaseDir()
      => AreEqual("/Foo",
                  Projects(Project("A")).SetValue(BaseDir, "/Foo").Get("A"/BaseDir));

    [Test]
    public void ProjectId_cannot_be_null_or_empty()
      => That(Throws<ArgumentNullException>(() => Project(null)).Message,
              Does.Contain("A project's ID must not be null or empty."));

    [Test]
    public void ProjectId_cannot_contain_slashes()
      => That(Throws<ArgumentException>(() => Project("A/B")).Message,
              Does.Contain("Project ID 'A/B' is invalid. It must not contain the character '/'."));
  }
}