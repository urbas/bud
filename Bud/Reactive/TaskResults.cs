using System;
using System.Reactive;
using System.Threading.Tasks;
using Bud.Util;
using static Bud.Util.Option;

namespace Bud.Reactive {
  public class TaskResults {
    private static readonly Type TaskGenericTypeDefinition = typeof(Task<object>).GetGenericTypeDefinition();

    public static Option<object> Await(Task task) {
      if (task == null) {
        return None<object>();
      }
      task.Wait();
      if (IsTaskWithResult(task)) {
        return task.GetType().GetProperty("Result").GetValue(task);
      }
      return Some<object>(Unit.Default);
    }

    public static bool IsTaskWithResult(Task task) {
      var type = task?.GetType();
      while (type != null) {
        if (type.IsGenericType &&
            type.GetGenericTypeDefinition() == TaskGenericTypeDefinition) {
          return true;
        }
        type = type.BaseType;
      }
      return false;
    }
  }
}