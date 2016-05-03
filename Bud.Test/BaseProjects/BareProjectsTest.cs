using Bud.Configuration;
using Bud.V1;
using NUnit.Framework;
using static NUnit.Framework.Assert;
using static System.IO.Path;
using static Bud.V1.Api;

namespace Bud.BaseProjects {
  public class BareProjectsTest {
    [Test]
    public void Set_ProjectId()
      => AreEqual("A", BareProject("A", "bar").Get(ProjectId));

    [Test]
    public void Set_ProjectDir()
      => AreEqual(Combine("/foo", "bar"),
                  BareProject("A", "bar", "/foo").Get(ProjectDir));

    [Test]
    public void Set_ProjectDir_from_ProjectId()
      => AreEqual(Combine("/foo", "A"),
                  BareProject("A", baseDir: "/foo").Get(ProjectDir));

    [Test]
    public void ProjectDir_must_be_relative_to_BaseDir()
      => AreEqual(Combine("/bar", "foo"),
                  BareProject("A", "foo", "/bar").Get(ProjectDir));

    [Test]
    public void ProjectDir_must_be_unchanged_if_absolute()
      => AreEqual("/bar",
                  BareProject("A", "/bar", "/foo").Get(ProjectDir));

    [Test]
    public void ProjectDir_is_BaseDir_when_empty()
      => AreEqual("/foo",
                  BareProject("A", "", "/foo").Get(ProjectDir));

    [Test]
    public void BaseDir_is_not_set_by_default()
      => Throws<ConfAccessException>(() => BareProject("A").Get(BaseDir));

    [Test]
    public void BuildDir_is_within_BaseDir()
      => AreEqual(Combine("/foo", "build", "A"),
                  BareProject("A", baseDir: "/foo").Get(BuildDir));

    [Test]
    public void ProjectVersion_is_initially_default()
      => AreEqual("0.0.1",
                  BareProject("A", "", "/foo").Get(ProjectVersion));

    [Test]
    public void Dependencies_should_be_initially_empty()
      => IsEmpty(BuildProject("A").Get(Dependencies));
  }
}