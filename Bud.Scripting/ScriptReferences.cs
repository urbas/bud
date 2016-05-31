using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using static Bud.Option;

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
    public static IImmutableSet<ScriptReference> Extract(string scriptContent) {
      var listBuilder = ImmutableList.CreateBuilder<ScriptReference>();
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

          var components = line.Split(new [] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
          if (components.Length == 2) {
            listBuilder.Add(new ScriptReference(components[1], None<string>()));
          } else if (components.Length == 3) {
            listBuilder.Add(new ScriptReference(components[1], components[2]));
          } else {
            throw new Exception($"Malformed reference: '{line}'. Should be of form: '//!reference <name> [version]'");
          }
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
    public static IImmutableSet<ScriptReference> Extract(IEnumerable<string> scriptContents)
      => scriptContents.Select(Extract)
                       .Aggregate((refs, refsUnion) => refsUnion.Union(refs));
  }
}