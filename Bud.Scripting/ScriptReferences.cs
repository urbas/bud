using System.Collections.Immutable;
using System.IO;

namespace Bud.Scripting {
  public static class ScriptReferences {
    public static IImmutableList<string> Parse(string scriptContent) {
      var listBuilder = ImmutableList.CreateBuilder<string>();
      using (var stringReader = new StringReader(scriptContent)) {
        while (true) {
          var line = stringReader.ReadLine();
          if (line == null) {
            break;
          }
          var trimmed = line.Trim();
          if (!string.IsNullOrEmpty(trimmed) && !trimmed.StartsWith("//")) {
            break;
          }
          if (!trimmed.StartsWith("//!reference ")) {
            continue;
          }
          var lastSpace = line.LastIndexOf(' ');
          listBuilder.Add(line.Substring(lastSpace + 1));
        }
      }
      return listBuilder.ToImmutable();
    }
  }
}