using System;

namespace Bud.Tasking.ApiV1 {
  static internal class TasksUtils {
    internal static void AssertTaskTypeIsSame<T>(string taskName, Type returnType) {
      if (returnType != typeof(T)) {
        throw new TaskReturnTypeException($"Could not treat the type of task '{taskName}' as '{returnType}'. Its actual type is '{typeof(T)}'.");
      }
    }
  }
}