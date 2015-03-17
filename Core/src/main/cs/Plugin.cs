namespace Bud {
  public abstract class Plugin {
    public abstract Settings Setup(Settings settings);

    public static implicit operator Setup(Plugin plugin) => plugin.Setup;
  }
}