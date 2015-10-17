using System;
using NUnit.Framework;

namespace Bud.IO {
  public class TimestampedTest {
    private readonly Timestamped<int> fooAtTime1 = Timestamped.Create(42, DateTimeOffset.FromFileTime(1));
    private readonly Timestamped<int> fooAtTime2 = Timestamped.Create(42, DateTimeOffset.FromFileTime(2));
    private readonly Timestamped<int> barAtTime1 = Timestamped.Create(9001, DateTimeOffset.FromFileTime(1));

    [Test]
    public void Contains_timestamped_value()
      => Assert.AreEqual(42, fooAtTime1.Value);

    [Test]
    public void Contains_timestamp()
      => Assert.AreEqual(DateTimeOffset.FromFileTime(1), fooAtTime1.Timestamp);

    [Test]
    public void Equals_when_values_equal()
      => Assert.IsTrue(fooAtTime1.Equals(fooAtTime2));

    [Test]
    public void Not_equals_when_values_differ()
      => Assert.IsFalse(barAtTime1.Equals(fooAtTime1));

    [Test]
    public void Operator_equals_when_values_equal()
      => Assert.IsTrue(fooAtTime1 == fooAtTime2);

    [Test]
    public void Operator_not_equals_when_values_differ()
      => Assert.IsFalse(barAtTime1 == fooAtTime1);

    [Test]
    public void Hash_equals_when_values_hashes_equal()
      => Assert.AreEqual(fooAtTime1.GetHashCode(), fooAtTime2.GetHashCode());

    [Test]
    public void Hash_differs_when_values_hashes_differ()
      => Assert.AreNotEqual(fooAtTime1.GetHashCode(), barAtTime1.GetHashCode());
  }
}