namespace Bud.ScriptToCsProj {
  public struct CsProjReference {
    public CsProjReference(string assemblyName, Option<string> path = default(Option<string>)) {
      Path = path;
      AssemblyName = assemblyName;
    }

    public string AssemblyName { get; }
    public Option<string> Path { get; }
  }
}