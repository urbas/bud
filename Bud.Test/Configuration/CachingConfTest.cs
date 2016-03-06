using System;
using System.Threading.Tasks;
using Bud.V1;
using Moq;
using NUnit.Framework;
using Bud.Util;
using static Bud.Util.Option;

namespace Bud.Configuration {
  public class CachingConfTest {
    private Mock<Func<Key<int>, Option<int>>> intFunc;
    private Mock<Func<Key<object>, Option<object>>> objectFunc;
    private CachingConf cachingConf;

    [SetUp]
    public void SetUp() {
      intFunc = new Mock<Func<Key<int>, Option<int>>>();
      intFunc.Setup(self => self("foo")).Returns(42);
      objectFunc = new Mock<Func<Key<object>, Option<object>>>();
      objectFunc.Setup(self => self("bar")).Returns<Key<object>>(k => new object());
      objectFunc.Setup(self => self("undefined")).Returns(None<object>());
      objectFunc.Setup(self => self("defined")).Returns(Some<object>("42"));
      cachingConf = new CachingConf();
    }

    [Test]
    public void Delegate_to_wrapped_conf() {
      cachingConf.TryGet("foo", intFunc.Object);
      intFunc.Verify(self => self("foo"));
    }

    [Test]
    public void Return_the_value_returned_by_wrapped_conf()
      => Assert.AreEqual(42, cachingConf.TryGet("foo", intFunc.Object).Value);

    [Test]
    public void Invokes_wrapped_conf_only_once() {
      cachingConf.TryGet("bar", intFunc.Object);
      cachingConf.TryGet("bar", intFunc.Object);
      intFunc.Verify(self => self("bar"), Times.Once);
    }

    [Test]
    public void Always_returns_the_same_value()
      => Assert.AreSame(cachingConf.TryGet("bar", objectFunc.Object).Value,
                        cachingConf.TryGet("bar", objectFunc.Object).Value);

    [Test]
    public void TryGet_returns_an_empty_optional_when_the_fallback_does_not_define_the_key()
      => Assert.IsFalse(cachingConf.TryGet("undefined", objectFunc.Object)
                                   .HasValue);

    [Test]
    public void TryGet_returns_an_optional_with_a_value_when_the_key_is_defined_by_the_fallback_function()
      => Assert.IsTrue(cachingConf.TryGet("defined", objectFunc.Object)
                                  .HasValue);

    [Test]
    public void TryGet_returns_an_optional_containing_the_value_returned_by_the_fallback_function()
      => Assert.AreEqual("42",
                         cachingConf.TryGet("defined", objectFunc.Object)
                                    .Value);

    [Test]
    public void Nested_concurrent_access_to_different_keys_must_not_deadlock() {
      var result = cachingConf.TryGet<int>("A", key => {
        var task = Task.Run(() => cachingConf.TryGet<int>("B", _ => 1).Value);
        task.Wait();
        return 42 + task.Result;
      });
      Assert.AreEqual(43, result.Value);
    }
  }
}