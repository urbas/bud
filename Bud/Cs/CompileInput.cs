using System;
using System.Collections.Generic;
using System.Linq;
using Bud.IO;

namespace Bud.Cs {
  public struct CompileInput {
    public List<string> Sources { get; }
    public List<string> Assemblies { get; }

    public CompileInput(List<string> sources, List<string> dependencies) {
      Sources = sources;
      Assemblies = dependencies;
    }

    public static CompileInput FromInOut(InOut input) {
      var sources = new List<string>();
      var assemblies = new List<string>();
      foreach (var element in input.Elements) {
        var assembly = element as Assembly;
        if (assembly != null) {
          assemblies.Add(assembly.Path);
          continue;
        }
        var file = element as InOutFile;
        if (file != null) {
          sources.Add(file.Path);
          continue;
        }
        throw new NotSupportedException($"Unknown input of type \"{input.Elements.Single().GetType().FullName}\".");
      }
      return new CompileInput(sources, assemblies);
    }
  }
}