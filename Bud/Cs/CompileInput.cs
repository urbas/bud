using System;
using System.Collections.Generic;
using System.Linq;
using Bud.IO;

namespace Bud.Cs {
  public struct CompileInput {
    public static void ExtractInput(InOut inOutInput, out List<Timestamped<string>> sources, out List<Timestamped<string>> assemblies) {
      sources = new List<Timestamped<string>>();
      assemblies = new List<Timestamped<string>>();
      foreach (var element in inOutInput.Elements) {
        var assembly = element as Assembly;
        if (assembly != null) {
          assemblies.Add(Files.ToTimestampedFile(assembly.Path));
          continue;
        }
        var file = element as InOutFile;
        if (file != null) {
          sources.Add(Files.ToTimestampedFile(file.Path));
          continue;
        }
        throw new NotSupportedException($"Unknown input of type \"{inOutInput.Elements.Single().GetType().FullName}\".");
      }
    }
  }
}