using System.Collections.Generic;
using Bud.IO;
using Microsoft.CodeAnalysis.CSharp;

namespace Bud.Cs {
  public interface ICompiler {
    CSharpCompilation Compile(IEnumerable<Timestamped<string>> inputSources,
                              IEnumerable<Timestamped<string>> inputAssemblies);
  }
}