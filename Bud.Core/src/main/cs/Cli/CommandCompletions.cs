namespace Bud.Cli {
  public static class CommandCompletions {
    public static string ExtractPartialCommand(string line, int cursorPosition) {
      return line.Trim().Substring(0, cursorPosition);
    }
  }
}