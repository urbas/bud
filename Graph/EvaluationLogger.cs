using System;

namespace Bud {
  public class EvaluationLogger : MarshalByRefObject {
    public void Log(LogLevel level, Scope scope, string content) {
      Console.WriteLine(level.ToString() + "@" + scope.ToString() + ": " + content);
    }
  }
}
