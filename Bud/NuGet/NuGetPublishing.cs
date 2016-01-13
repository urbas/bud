using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reactive.Linq;
using Bud.V1;
using static System.IO.Path;
using static System.Linq.Enumerable;
using static Bud.V1.Api;

namespace Bud.NuGet {
  internal static class NuGetPublishing {
    internal static Conf NuGetPublishingSupport
      = Conf.Empty
            .Init(PackageMetadata, DefaultPackageMetadata)
            .Init(PackageOutputDir, DefaultPackageOutputDir)
            .Init(Publish, DefaultPublish)
            .Init(Package, DefaultPackage)
            .Init(PackageFiles, DefaultPackageFiles);

    private static NuGetPackageMetadata DefaultPackageMetadata(IConf c)
      => new NuGetPackageMetadata(Environment.UserName,
                                  ProjectId[c],
                                  ImmutableDictionary<string, string>.Empty);

    private static string DefaultPackageOutputDir(IConf c)
      => Combine(TargetDir[c], PackageOutputDirName);

    private static IObservable<string> DefaultPackage(IConf c)
      => PackageFiles[c].Select(packageFiles => Packager[c].Pack(
        PackageOutputDir[c],
        ProjectId[c],
        Api.Version[c],
        packageFiles,
        PackageDependencies(c),
        PackageMetadata[c]));

    private static IObservable<IEnumerable<PackageFile>> DefaultPackageFiles(IConf c)
      => Output[c].Select(files => files.Select(ToContentFiles));

    private static IObservable<bool> DefaultPublish(IConf c)
      => Package[c].Select(package => Publisher[c].Publish(package));

    private static PackageFile ToContentFiles(string file)
      => new PackageFile(file, $"content/{GetFileName(file)}");

    private static IEnumerable<PackageDependency> PackageDependencies(IConf c)
      => Dependencies[c].Select(dependency => ToPackageDependency(c, dependency));

    private static PackageDependency ToPackageDependency(IConf c, string dependency)
      => new PackageDependency(c.Get(dependency/ProjectId),
                               c.Get(dependency/Api.Version));
  }
}