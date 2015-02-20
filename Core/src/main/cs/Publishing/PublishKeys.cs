using NuGet;

namespace Bud.Publishing {
  public static class PublishKeys {
    public static readonly TaskKey Publish = TaskKey.Define("publish", "Publishes NuGet packages to the designated NuGet repository.");
    public static readonly TaskKey<string> Package =  TaskKey<string>.Define("package", "Creates a NuGet package and returns the path to the created package.");
    public static readonly ConfigKey<string> PublishApiKey = Publish / ConfigKey<string>.Define("apiKey", "The API key for the NuGet server to which to publish the package.");
  }
}