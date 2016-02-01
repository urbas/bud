using NUnit.Framework;
using static NUnit.Framework.Assert;
using static System.IO.Path;
using static Bud.V1.Api;

namespace Bud.BaseProjects {
  public class BareProjectsTest {
    [Test]
    public void Set_the_projectDir()
      => AreEqual("bar", ProjectDir[BareProject("bar", "Foo")]);

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
  }
}