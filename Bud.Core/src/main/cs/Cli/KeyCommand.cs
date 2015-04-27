namespace Bud.Cli {
  public class KeyCommand : Command {
    public string Key { get; }

    public KeyCommand(string key) {
      Key = key;
    }
  }
}