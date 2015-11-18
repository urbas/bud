using Moq;
using NUnit.Framework;

namespace Bud.Configuration {
  public class CachingConfTest {
    private Mock<ConfValueCalculator> wrappedConf;
    private CachingConf cachingConf;

    [SetUp]
    public void SetUp() {
      wrappedConf = new Mock<ConfValueCalculator>();
      wrappedConf.Setup(self => self.Get<int>("foo", It.IsAny<IConf>())).Returns(42);
      wrappedConf.Setup(self => self.Get<object>("bar", It.IsAny<IConf>())).Returns<Key<object>, IConf>((k, c) => new object());
      cachingConf = new CachingConf(wrappedConf.Object);
    }

    [Test]
    public void Delegate_to_wrapped_conf() {
      cachingConf.Get<int>("foo");
      wrappedConf.Verify(self => self.Get<int>("foo", It.IsAny<IConf>()));
    }

    [Test]
    public void Return_the_value_returned_by_wrapped_conf()
      => Assert.AreEqual(42, cachingConf.Get<int>("foo"));

    [Test]
    public void Invokes_wrapped_conf_only_once() {
      cachingConf.Get<int>("bar");
      cachingConf.Get<int>("bar");
      wrappedConf.Verify(self => self.Get<int>("bar", It.IsAny<IConf>()), Times.Once);
    }

    [Test]
    public void Always_returns_the_same_value()
      => Assert.AreSame(cachingConf.Get<object>("bar"),
                        cachingConf.Get<object>("bar"));
  }
}