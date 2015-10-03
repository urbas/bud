using Bud.Tasking;
using NUnit.Framework;

namespace Bud.Take2 {
  public class BuildTest {
    private readonly Tasks fooBarProject = Build.Project("foo", "Foo.Bar");

    [Test]
    public async void should_set_the_projectDir() {
      Assert.AreEqual("foo", await fooBarProject.Invoke<string>("projectDir"));
    }

    [Test]
    public async void should_set_the_projectId() {
      Assert.AreEqual("Foo.Bar", await fooBarProject.Invoke<string>("projectId"));
    }
  }
}