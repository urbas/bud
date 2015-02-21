using NuGet;

namespace Bud.Publishing {
  public static class PublishKeys {
    public static readonly TaskKey Publish = Key.Define("publish", "Publishes NuGet packages to the designated NuGet repository.");
    public static readonly TaskKey<string> Package = Key.Define("package", "Creates a NuGet package and returns the path to the created package.");
    public static readonly ConfigKey<string> PublishApiKey = Publish / Key.Define("apiKey", "The API key for the NuGet server to which to publish the package.");
  }
}