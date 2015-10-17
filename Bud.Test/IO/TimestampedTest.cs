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
    public void Operator_not_equals_is_false_when_values_equal()
      => Assert.IsFalse(fooAtTime1 != fooAtTime2);

    [Test]
    public void Operator_not_equals_is_true_when_values_differ()
      => Assert.IsTrue(barAtTime1 != fooAtTime1);

    [Test]
    public void Hash_equals_the_hash_to_value()
      => Assert.AreEqual(fooAtTime1.GetHashCode(), fooAtTime1.Value.GetHashCode());

    [Test]
    public void Equals_to_own_value()
      => Assert.IsTrue(fooAtTime1.Equals(fooAtTime1.Value));

    [Test]
    public void Not_equals_to_different_value()
      => Assert.IsFalse(fooAtTime1.Equals(barAtTime1.Value));

    [Test]
    public void Operator_equals_is_true_when_value_equals()
      => Assert.IsTrue(fooAtTime1 == fooAtTime1.Value);

    [Test]
    public void Operator_equals_is_false_when_value_differs()
      => Assert.IsFalse(fooAtTime1 == barAtTime1.Value);

    [Test]
    public void Operator_not_equals_is_false_when_value_equals()
      => Assert.IsFalse(fooAtTime1 != fooAtTime1.Value);

    [Test]
    public void Operator_not_equals_is_false_when_value_differs()
      => Assert.IsTrue(fooAtTime1 != barAtTime1.Value);
  }
}