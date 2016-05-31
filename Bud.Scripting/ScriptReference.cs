namespace Bud.Scripting {
  public struct ScriptReference {
    public string Name { get; }
    public Option<string> Version { get; }

    public ScriptReference(string name, Option<string> version) {
      Name = name;
      Version = version;
    }

    public override string ToString() => $"ScriptReference({Name}, {Version})";
  }
}