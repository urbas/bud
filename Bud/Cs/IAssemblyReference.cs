using Bud.IO;
using Microsoft.CodeAnalysis;

namespace Bud.Cs {
  public interface IAssemblyReference {
    MetadataReference MetadataReference { get; }
    Timestamped<IAssemblyReference> ToTimestamped();
  }
}