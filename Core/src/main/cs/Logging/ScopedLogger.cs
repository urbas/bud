namespace Bud.Logging {
  internal class ScopedLogger : ILogger {
    private readonly ILogger Logger;
    private readonly Key TaskKey;

    public ScopedLogger(ILogger logger, Key taskKey) {
      Logger = logger;
      TaskKey = taskKey;
    }

    public void Info(string message) {
      Logger.Info(TaskKey + "> " + message);
    }

    public void Error(string message) {
      Logger.Error(TaskKey + "> " + message);
    }
  }
}