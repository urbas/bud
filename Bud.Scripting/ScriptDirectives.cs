using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.IO;
using static Bud.Option;

namespace Bud.Scripting {
  public class ScriptDirectives {
    public IImmutableSet<string> References { get; }
    public IReadOnlyDictionary<string, Option<string>> NuGetReferences { get; }

    public ScriptDirectives(IImmutableSet<string> references,
                            IReadOnlyDictionary<string, Option<string>> nuGetReferences) {
      References = references;
      NuGetReferences = nuGetReferences;
    }

    public static ScriptDirectives Extract(params string[] scriptContents)
      => Extract((IEnumerable<string>) scriptContents);

    /// <summary>
    ///   Extracts directives from the contents of the scripts. Directives are of the following forms:
    ///   - <c>//!reference [ASSEMBLY NAME]</c>
    ///   - <c>//!nuget [NUGET PACKAGE ID] [VERSION]</c>
    /// </summary>
    /// <param name="scriptContents">textual contents of the script files.</param>
    /// <returns>
    ///   an instance of the <see cref="ScriptDirectives" /> class. This class contains
    ///   all the directives found in the scripts.
    /// </returns>
    public static ScriptDirectives Extract(IEnumerable<string> scriptContents) {
      var assemblyReferences = new List<string>();
      var nugetReferences = new Dictionary<string, Option<string>>();
      foreach (var scriptContent in scriptContents) {
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
            if (trimmed.StartsWith("//!reference ")) {
              assemblyReferences.Add(ToAssemblyReference(line));
            }
            if (trimmed.StartsWith("//!nuget ")) {
              var nuGetReference = ToNuGetReference(line);
              nugetReferences[nuGetReference.Item1] = nuGetReference.Item2;
            }
          }
        }
      }

      return new ScriptDirectives(assemblyReferences.ToImmutableHashSet(),
                                  new ReadOnlyDictionary<string, Option<string>>(nugetReferences));
    }

    private static string ToAssemblyReference(string line) {
      var components = line.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
      if (components.Length == 2) {
        return components[1];
      }
      throw new Exception($"Malformed reference: '{line}'. Should be of form: '//!reference <name>'");
    }

    private static Tuple<string, Option<string>> ToNuGetReference(string line) {
      var components = line.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
      if (components.Length == 2) {
        return Tuple.Create(components[1], None<string>());
      }
      if (components.Length == 3) {
        return Tuple.Create(components[1], Some(components[2]));
      }
      throw new Exception($"Malformed NuGet package reference: '{line}'. Should be of form: '//!nuget <name> [version]'");
    }
  }
}