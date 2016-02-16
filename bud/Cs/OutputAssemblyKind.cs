using Microsoft.CodeAnalysis;

namespace Bud.Cs {
  public static class OutputAssemblyKind {
    public static string ToExtension(this OutputKind kind) {
      switch (kind) {
        case OutputKind.ConsoleApplication:
        case OutputKind.WindowsApplication:
        case OutputKind.WindowsRuntimeApplication:
          return ".exe";
        case OutputKind.DynamicallyLinkedLibrary:
          return ".dll";
        case OutputKind.NetModule:
          return ".netmodule";
        case OutputKind.WindowsRuntimeMetadata:
          return ".winmdobj";
        default:
          return ".dll";
      }
    }
  }
}