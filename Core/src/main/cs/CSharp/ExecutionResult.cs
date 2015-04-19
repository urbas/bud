using System;

namespace Bud.CSharp {
  public class ExecutionResult {
    public const int SuccessExitCode = 0;
    public int ExitCode { get; }
    public string Output { get; }
  }
}