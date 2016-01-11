using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using Bud.V1;
using static System.IO.Path;
using static System.Linq.Enumerable;
using static Bud.V1.Api;

namespace Bud.NuGet {
  internal static class NuGetPublishing {
    internal static Conf NuGetPublishingSupport
      = Conf.Empty
            .Init(Publish, DefaultPublish)
            .Init(PackageFiles, DefaultPackageFiles);

    private static IObservable<IEnumerable<PackageFile>> DefaultPackageFiles(IConf c)
      => Output[c].Select(files => files.Select(ToContentFiles));

    private static IObservable<Unit> DefaultPublish(IConf c) {
      return PackageFiles[c].Select(packageFiles => {
        Publisher[c].Publish(ProjectId[c],
                                  Api.Version[c],
                                  packageFiles,
                                  PackageDependencies(c));
        return Unit.Default;
      });
    }

    private static PackageFile ToContentFiles(string file)
      => new PackageFile(file, $"content/{GetFileName(file)}");

    private static IEnumerable<PackageDependency> PackageDependencies(IConf c)
      => Dependencies[c].Select(dependency => ToPackageDependency(c, dependency));

    private static PackageDependency ToPackageDependency(IConf c, string dependency)
      => new PackageDependency(c.Get(dependency/ProjectId),
                               c.Get(dependency/Api.Version));
  }
}