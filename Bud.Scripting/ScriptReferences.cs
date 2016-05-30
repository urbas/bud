using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace Bud.Scripting {
  public static class ScriptReferences {
    /// <summary>
    ///   Parses the top part of the script and returns references it finds.
    ///   A reference is of the form <c>//!reference Foo.Bar</c>.
    /// </summary>
    /// <param name="scriptContent">the C# script from which to extract references.</param>
    /// <returns>
    ///   A set of references.
    /// </returns>
    public static IImmutableSet<string> Extract(string scriptContent) {
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
      return listBuilder.ToImmutableHashSet();
    }

    /// <param name="scriptContents">
    ///   String contents of all the scripts from which to extract references.
    /// </param>
    /// <returns>
    ///   All assembly references from the given scripts.
    /// </returns>
    public static IImmutableSet<string> Extract(IEnumerable<string> scriptContents)
      => scriptContents.Select(Extract)
                       .Aggregate((refs, refsUnion) => refsUnion.Union(refs));
  }
}