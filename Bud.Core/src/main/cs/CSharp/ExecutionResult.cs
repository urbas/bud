namespace Bud.CSharp {
  public class ExecutionResult {
    public const int SuccessExitCode = 0;

    public ExecutionResult(int exitCode, string output, string errorOutput) {
      ExitCode = exitCode;
      Output = output;
      ErrorOutput = errorOutput;
    }

    public int ExitCode { get; }
    public string Output { get; }
    public string ErrorOutput { get; }
  }
}