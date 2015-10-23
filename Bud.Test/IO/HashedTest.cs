using NUnit.Framework;

namespace Bud.IO {
  public class HashedTest {
    private readonly Hashed<int> fooAtTime1 = Hashed.Create(42, 1);
    private readonly Hashed<int> fooAtTime2 = Hashed.Create(42, 2);
    private readonly Hashed<int> barAtTime1 = Hashed.Create(9001, 1);

    [Test]
    public void Contains_value()
      => Assert.AreEqual(42, fooAtTime1.Value);

    [Test]
    public void Contains_hash()
      => Assert.AreEqual(1, fooAtTime1.Hash);

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
  }
}