using System.Reactive;
using System.Reactive.Linq;
using Bud.V1;
using static Bud.V1.Api;

namespace Bud.NuGet {
  internal static class NuGetPublishing {
    internal static Conf NuGetPublishingSupport
      = Conf.Empty
            .Init(Publish, DefaultPublish);

    public static Key<INuGetPublisher> NuGetPublisher = nameof(NuGetPublisher);

    private static Unit DefaultPublish(IConf c) {
      NuGetPublisher[c].Publish(ProjectId[c], Version[c], Output[c].Take(1).Wait());
      return Unit.Default;
    }
  }
}