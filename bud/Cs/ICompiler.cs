using System.Collections.Generic;
using Bud.IO;

namespace Bud.Cs {
  public interface ICompiler {
    CompileOutput Compile(IEnumerable<Timestamped<string>> inputSources,
                          IEnumerable<Timestamped<string>> inputAssemblies,
                          string outputAssemblyPath);
  }
}