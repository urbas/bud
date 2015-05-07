using System.Collections.Generic;

namespace Bud.Cli {
  public class ConsoleHistory {
    private readonly List<string> History = new List<string> {string.Empty};
    private int CurrentHistoryIndex;

    public void StoreAsLastEntry(LineEditor lineEditor)
      => History[History.Count - 1] = lineEditor.Line;

    public void AddEntry(LineEditor lineEditor) {
      CurrentHistoryIndex = History.Count;
      History.Add(lineEditor.Line);
    }

    public void UpdateCurrentEntry(LineEditor lineEditor) 
      => History[CurrentHistoryIndex] = lineEditor.Line;

    public void GoToPreviousEntry(LineEditor lineEditor) {
      UpdateCurrentEntry(lineEditor);
      if (CurrentHistoryIndex > 0) {
        lineEditor.Line = History[--CurrentHistoryIndex];
      }
    }

    public void GoToNextEntry(LineEditor lineEditor) {
      UpdateCurrentEntry(lineEditor);
      if (CurrentHistoryIndex < History.Count - 1) {
        lineEditor.Line = History[++CurrentHistoryIndex];
      }
    }
  }
}