using NuGet;

namespace Bud.Publishing {
  public static class PublishKeys {
    public static readonly TaskKey Publish = new TaskKey("publish", "Publishes NuGet packages to the designated NuGet repository.");
    public static readonly TaskKey<string> Package =  new TaskKey<string>("package", "Creates a NuGet package and returns the path to the created package.");
    public static readonly ConfigKey<string> PublishApiKey = new ConfigKey<string>("apiKey", "The API key for the NuGet server to which to publish the package.").In(Publish);
  }
}