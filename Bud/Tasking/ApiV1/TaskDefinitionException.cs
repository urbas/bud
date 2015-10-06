using System;

namespace Bud.Tasking.ApiV1 {
  public class TaskDefinitionException : Exception {
    public string TaskName { get; }
    public Type ResultType { get; }

    public TaskDefinitionException(string taskName, Type resultType, string message) : base(message) {
      TaskName = taskName;
      ResultType = resultType;
    }
  }
}