using Bud.Tasking;
using NUnit.Framework;
using static Bud.Take2.Build;

namespace Bud.Take2 {
  public class BuildTest {
    private readonly Tasks fooProject = Project("foo", "Foo");

    [Test]
    public async void should_set_the_projectDir() {
      Assert.AreEqual("foo", await ProjectDir[fooProject]);
    }

    [Test]
    public async void should_set_the_projectId() {
      Assert.AreEqual("Foo", await ProjectId[fooProject]);
    }
  }
}