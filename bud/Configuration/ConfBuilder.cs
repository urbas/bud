namespace Bud.Configuration {
  public abstract class ConfBuilder : IConfBuilder {
    protected ConfBuilder(string path) {
      Path = path;
    }

    public string Path { get; }
    public abstract void AddTo(ConfDirectory confDirectory);
  }
}