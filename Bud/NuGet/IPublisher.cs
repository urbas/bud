namespace Bud.NuGet {
  public interface IPublisher {
    bool Publish(string package);
  }
}