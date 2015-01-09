using System;

public class ATest {
  public void Message_MUST_say_the_right_thing() {
    if (!"I am the A class!".Equals(A.Message)) {
        throw new Exception("Class A did not print the right message.");
    }
  }
}