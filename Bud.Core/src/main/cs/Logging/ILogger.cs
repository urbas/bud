namespace Bud.Logging {
  public interface ILogger {
    void Info(string message);
    void Error(string message);
  }
}