using System.Collections.Generic;
using System.Diagnostics;
using Bud.IO;

namespace Bud.Cs {
  public interface ICompiler {
    CompileOutput Compile(IEnumerable<Timestamped<string>> inputSources,
                          IEnumerable<Timestamped<string>> inputAssemblies,
                          string outputAssemblyPath,
                          Stopwatch stopwatch);
  }
}