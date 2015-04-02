using System;
using NUnit.Framework;

public class ATest {
  [Test]
  public void Message_MUST_say_the_right_thing() {
    Assert.AreEqual("I am the A class!", A.Message);
  }
}