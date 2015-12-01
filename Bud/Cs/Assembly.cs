using Bud.IO;

namespace Bud.Cs {
  public class Assembly : InOutFile {
    public Assembly(string path, bool isOkay) : base(path, isOkay) {}

    public static Assembly ToAssembly(string dllPath) => ToAssembly(dllPath, true);

    public static Assembly ToAssembly(string dllPath, bool isOkay)
      => new Assembly(dllPath, isOkay);
  }
}