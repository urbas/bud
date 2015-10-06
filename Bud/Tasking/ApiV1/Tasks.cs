using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Bud.Tasking.ApiV1 {
  public struct Tasks : IEnumerable<ITaskModification> {
    public static readonly Tasks NewTasks = new Tasks(Enumerable.Empty<ITaskModification>());
    private IEnumerable<ITaskModification> TaskModifications { get; }

    public Tasks(IEnumerable<ITaskModification> taskModifications) {
      TaskModifications = taskModifications;
    }

    public IEnumerator<ITaskModification> GetEnumerator() {
      return TaskModifications.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return ((IEnumerable) TaskModifications).GetEnumerator();
    }
  }
}