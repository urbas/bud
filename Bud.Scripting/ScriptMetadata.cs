using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.IO;
using static Bud.Option;

namespace Bud.Scripting {
  public class ScriptMetadata {
    public static ScriptMetadata Extract(params string[] scriptContents) 
      => Extract((IEnumerable<string>)scriptContents);

    public static ScriptMetadata Extract(IEnumerable<string> scriptContents) {
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

      return new ScriptMetadata(assemblyReferences.ToImmutableHashSet(),
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


    public ScriptMetadata(IImmutableSet<string> assemblyReferences, IReadOnlyDictionary<string, Option<string>> nuGetReferences) {
      AssemblyReferences = assemblyReferences;
      NuGetReferences = nuGetReferences;
    }

    public IImmutableSet<string> AssemblyReferences { get; }
    public IReadOnlyDictionary<string, Option<string>> NuGetReferences { get; }
  }
}