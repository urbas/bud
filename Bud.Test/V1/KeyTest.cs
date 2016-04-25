using NUnit.Framework;
using static NUnit.Framework.Assert;

namespace Bud.V1 {
  public class KeyTest {
    [Test]
    public void Joining_root_with_an_untyped_relative_key()
      => AreEqual(new Key("/A"), Keys.Root/"A");

    [Test]
    public void Joining_root_with_a_typed_key()
      => AreEqual(new Key<int>("/A"), Keys.Root/new Key<int>("A"));
  }
}