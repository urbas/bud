using NUnit.Framework;

namespace Bud.IO {
  public class TimestampedTest {
    private readonly Timestamped<int> fooAtTime1 = Timestamped.Create(42, 1);
    private readonly Timestamped<int> fooAtTime2 = Timestamped.Create(42, 2);
    private readonly Timestamped<int> barAtTime1 = Timestamped.Create(9001, 1);

    [Test]
    public void Contains_value()
      => Assert.AreEqual(42, fooAtTime1.Value);

    [Test]
    public void Contains_hash()
      => Assert.AreEqual(1, fooAtTime1.Timestamp);

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
    public void Hash_code_equals_the_hash_code_of_the_value()
      => Assert.AreEqual(fooAtTime1.GetHashCode(), fooAtTime1.Value.GetHashCode());

    [Test]
    public void Equals_to_own_value()
      => Assert.IsTrue(fooAtTime1.Equals(fooAtTime1.Value));

    [Test]
    public void Not_equals_to_different_value()
      => Assert.IsFalse(fooAtTime1.Equals(barAtTime1.Value));

    [Test]
    public void Operator_equals_same_value()
      => Assert.IsTrue(fooAtTime1 == fooAtTime1.Value);

    [Test]
    public void Operator_equals_different_value()
      => Assert.IsFalse(fooAtTime1 == barAtTime1.Value);

    [Test]
    public void Operator_not_equals_same_value()
      => Assert.IsFalse(fooAtTime1 != fooAtTime1.Value);

    [Test]
    public void Operator_not_equals_different_value()
      => Assert.IsTrue(fooAtTime1 != barAtTime1.Value);

    [Test]
    public void IsUpToDateWith_returns_true_when_the_other_resource_has_the_same_timestamp()
      => Assert.IsTrue(fooAtTime1.IsUpToDateWith(barAtTime1));

    [Test]
    public void IsUpToDateWith_returns_true_when_the_other_resource_has_an_older_timestamp()
      => Assert.IsTrue(fooAtTime2.IsUpToDateWith(fooAtTime1));

    [Test]
    public void IsUpToDateWith_returns_false_when_the_other_resource_has_a_newer_timestamp()
      => Assert.IsFalse(fooAtTime1.IsUpToDateWith(fooAtTime2));

    [Test]
    public void IsUpToDateWith_returns_true_when_all_other_resources_are_older()
      => Assert.IsTrue(fooAtTime1.IsUpToDateWith(new[] {Timestamped.Create("foo", 0)}));

    [Test]
    public void IsUpToDateWith_returns_true_when_all_other_resources_have_an_equal_timestamp()
      => Assert.IsTrue(fooAtTime1.IsUpToDateWith(new[] {Timestamped.Create("foo", 1)}));

    [Test]
    public void IsUpToDateWith_returns_false_when_any_resource_is_newer()
      => Assert.IsFalse(fooAtTime1.IsUpToDateWith(new[] {Timestamped.Create("foo", 2)}));
  }
}