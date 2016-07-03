using System;
using System.Threading;

namespace Bud {
  public static class TaskTestUtils {
    public static T InvokeAndWait<T>(Func<CountdownEvent, T> invokee,
                                     int waitCountdown = 1) {
      var barrier = new CountdownEvent(waitCountdown);
      var observedChanges = invokee(barrier);
      barrier.Wait();
      return observedChanges;
    }

    public static void InvokeAndWait(Action<CountdownEvent> invokee,
                                     int waitCountdown = 1) {
      var barrier = new CountdownEvent(waitCountdown);
      invokee(barrier);
      barrier.Wait();
    }
  }
}