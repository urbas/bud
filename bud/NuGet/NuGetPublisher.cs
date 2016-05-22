namespace Bud.NuGet {
  public class NuGetPublisher : IPublisher {
    public NuGetExecutable NuGetExecutable { get; }
    public NuGetPushArgsBuilder PushArgsBuilder { get; }

    public NuGetPublisher(NuGetExecutable executable = null,
                          NuGetPushArgsBuilder argsBuilder = null) {
      NuGetExecutable = executable ?? NuGetExecutable.Instance;
      PushArgsBuilder = argsBuilder ?? new NuGetPushArgsBuilder();
    }

    public bool Publish(string package, Option<string> targetUrl, Option<string> apiKey)
      => NuGetExecutable.Run(PushArgsBuilder.CreateArgs(package, targetUrl, apiKey));
  }
}