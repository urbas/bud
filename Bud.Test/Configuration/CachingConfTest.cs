using System;
using Moq;
using NUnit.Framework;

namespace Bud.Configuration {
  public class CachingConfTest {
    private Mock<Func<Key<int>, int>> intFunc;
    private Mock<Func<Key<object>, object>> objectFunc;
    private CachingConf cachingConf;

    [SetUp]
    public void SetUp() {
      intFunc = new Mock<Func<Key<int>, int>>();
      intFunc.Setup(self => self("foo")).Returns(42);
      objectFunc = new Mock<Func<Key<object>, object>>();
      objectFunc.Setup(self => self("bar")).Returns<Key<object>>(k => new object());
      cachingConf = new CachingConf();
    }

    [Test]
    public void Delegate_to_wrapped_conf() {
      cachingConf.Get("foo", intFunc.Object);
      intFunc.Verify(self => self("foo"));
    }

    [Test]
    public void Return_the_value_returned_by_wrapped_conf()
      => Assert.AreEqual(42, cachingConf.Get("foo", intFunc.Object));

    [Test]
    public void Invokes_wrapped_conf_only_once() {
      cachingConf.Get("bar", intFunc.Object);
      cachingConf.Get("bar", intFunc.Object);
      intFunc.Verify(self => self("bar"), Times.Once);
    }

    [Test]
    public void Always_returns_the_same_value()
      => Assert.AreSame(cachingConf.Get("bar", objectFunc.Object),
                        cachingConf.Get("bar", objectFunc.Object));
  }
}