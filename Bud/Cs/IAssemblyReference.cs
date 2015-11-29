using Bud.IO;
using Microsoft.CodeAnalysis;

namespace Bud.Cs {
  public interface IAssemblyReference {
    MetadataReference MetadataReference { get; }
    string Path { get; }
    Timestamped<IAssemblyReference> ToTimestamped();
  }
}