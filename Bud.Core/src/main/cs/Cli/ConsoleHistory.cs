using System.Collections.Generic;

namespace Bud.Cli {
  public class ConsoleHistory {
    private readonly List<string> History = new List<string>();
    private int CurrentHistoryIndex;
    public ConsoleHistory() {}

    public void AddHistoryEntry() {
      if (CurrentHistoryIndex < History.Count - 1) {
        History[History.Count - 1] = History[CurrentHistoryIndex];
      }
      CurrentHistoryIndex = History.Count;
      History.Add(string.Empty);
    }

    public void RecordHistoryEntry(LineEditor lineEditor) {
      History[CurrentHistoryIndex] = lineEditor.Line;
    }

    public void ShowPreviousHistoryEntry(LineEditor lineEditor) {
      if (CurrentHistoryIndex > 0) {
        lineEditor.Line = History[--CurrentHistoryIndex];
      }
    }

    public void ShowNextHistoryEntry(LineEditor lineEditor) {
      if (CurrentHistoryIndex < History.Count - 1) {
        lineEditor.Line = History[++CurrentHistoryIndex];
      }
    }
  }
}