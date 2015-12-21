using Bud.IO;

namespace Bud.Cs {
  public class Assembly : InOutFile {
    public Assembly(string path) : base(path) {}
    public static Assembly ToAssembly(string dllPath) => new Assembly(dllPath);
  }
}