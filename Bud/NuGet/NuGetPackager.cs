using System.Collections.Generic;
using System.IO;
using static System.IO.Directory;
using static System.IO.File;
using static System.IO.Path;
using static Bud.NuGet.NuGetExecutable;

namespace Bud.NuGet {
  public class NuGetPackager : IPackager {
    public string Pack(string outputDir, string baseDir, string packageId, string version, IEnumerable<PackageFile> files, IEnumerable<PackageDependency> packageDependencies, NuGetPackageMetadata packageMetadata) {
      var nuspecFile = Combine(outputDir, $"{packageId}.nuspec");
      CreateDirectory(outputDir);
      CreateNuspecFile(nuspecFile, packageId, version, packageDependencies, packageMetadata, files);
      ExecuteNuGet($"pack {nuspecFile} " +
                   $"-OutputDirectory {outputDir} " +
                   $"-BasePath {baseDir}");
      return Combine(outputDir, $"{packageId}.{version}.nupkg");
    }

    private static void CreateNuspecFile(string nuspecFile, string packageId, string version, IEnumerable<PackageDependency> packageDependencies, NuGetPackageMetadata packageMetadata, IEnumerable<PackageFile> files) {
      using (var stream = Open(nuspecFile, FileMode.Create, FileAccess.Write)) {
        using (var writer = new StreamWriter(stream)) {
          WriteNuspecContent(writer, packageId, version, packageDependencies, packageMetadata, files);
        }
      }
    }

    private static void WriteNuspecContent(TextWriter writer, string packageId, string version, IEnumerable<PackageDependency> packageDependencies, NuGetPackageMetadata packageMetadata, IEnumerable<PackageFile> files) {
      writer.Write("<?xml version=\"1.0\"?>");
      writer.Write("<package xmlns=\"http://schemas.microsoft.com/packaging/2011/08/nuspec.xsd\">");
      writer.Write("<metadata>");
      writer.Write($"<id>{packageId}</id>");
      writer.Write($"<version>{version}</version>");
      writer.Write($"<authors>{packageMetadata.Authors}</authors>");
      writer.Write($"<description>{packageMetadata.Description}</description>");
      foreach (var optionalField in packageMetadata.OptionalFields) {
        writer.Write($"<{optionalField.Key}>{optionalField.Value}</{optionalField.Key}>");
      }
      writer.Write("<dependencies>");
      foreach (var packageDependency in packageDependencies) {
        writer.Write($"<dependency id=\"{packageDependency.PackageId}\" version=\"{packageDependency.Version}\" />");
      }
      writer.Write("</dependencies>");
      writer.Write("</metadata>");
      writer.Write("<files>");
      foreach (var file in files) {
        writer.Write($"<file src=\"{file.FileToPackage}\" target=\"{file.PathInPackage}\" />");
      }
      writer.Write("</files>");
      writer.Write("</package>");
    }
  }
}