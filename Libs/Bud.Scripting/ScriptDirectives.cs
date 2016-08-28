using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Bud.NuGet;
using NuGet.Frameworks;
using NuGet.Versioning;

namespace Bud.Scripting {
  public class ScriptDirectives {
    public ImmutableArray<string> References { get; }
    public ImmutableArray<PackageReference> NuGetReferences { get; }

    public ScriptDirectives(ImmutableArray<string> references,
                            ImmutableArray<PackageReference> nuGetReferences) {
      References = references;
      NuGetReferences = nuGetReferences;
    }

    public static ScriptDirectives Extract(params string[] scriptContents)
      => Extract((IEnumerable<string>) scriptContents);

    /// <summary>
    ///   Extracts directives from the contents of the scripts. Directives are of the following forms:
    ///   - <c>//!reference [ASSEMBLY NAME | ASSEMBLY PATH]</c>
    ///   - <c>//!nuget [NUGET PACKAGE ID] [VERSION]</c>
    /// </summary>
    /// <param name="scriptContents">textual contents of the script files.</param>
    /// <returns>
    ///   an instance of the <see cref="ScriptDirectives" /> class. This class contains
    ///   all the directives found in the scripts.
    /// </returns>
    public static ScriptDirectives Extract(IEnumerable<string> scriptContents) {
      var assemblyReferences = ImmutableArray.CreateBuilder<string>();
      var nugetReferences = ImmutableArray.CreateBuilder<PackageReference>();
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
              nugetReferences.Add(ToNuGetReference(line));
            }
          }
        }
      }
      return new ScriptDirectives(assemblyReferences.ToImmutable(), nugetReferences.ToImmutable());
    }

    private static string ToAssemblyReference(string line) {
      var components = line.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
      if (components.Length == 2) {
        return components[1];
      }
      throw new Exception($"Malformed reference: '{line}'. Should be of form: '//!reference <name or path>'");
    }

    private static PackageReference ToNuGetReference(string line) {
      var components = line.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
      if (components.Length == 3) {
        return new PackageReference(components[1], NuGetVersion.Parse(components[2]), NuGetFramework.AnyFramework);
      }
      throw new Exception($"Malformed NuGet package reference: '{line}'. " +
                          $"Should be of form: '//!nuget <name> <version>'");
    }
  }
}