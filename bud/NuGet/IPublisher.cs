using Bud.Util;

namespace Bud.NuGet {
  public interface IPublisher {
    bool Publish(string package, Option<string> targetUrl, Option<string> apiKey);
  }
}