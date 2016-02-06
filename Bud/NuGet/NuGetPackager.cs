using System;
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
      var arguments = $"pack {nuspecFile} " +
                      $"-OutputDirectory {outputDir} " +
                      $"-BasePath {baseDir} " +
                      "-NonInteractive";
      Console.WriteLine($"Arguments: {arguments}");
      ExecuteNuGet(arguments);
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
      writer.Write("<?xml version=\"1.0\"?>\n");
      writer.Write("<package xmlns=\"http://schemas.microsoft.com/packaging/2011/08/nuspec.xsd\">\n");
      writer.Write("  <metadata>\n");
      writer.Write($"    <id>{packageId}</id>\n");
      writer.Write($"    <version>{version}</version>\n");
      writer.Write($"    <authors>{packageMetadata.Authors}</authors>\n");
      writer.Write($"    <description>{packageMetadata.Description}</description>\n");
      foreach (var optionalField in packageMetadata.OptionalFields) {
        writer.Write($"    <{optionalField.Key}>{optionalField.Value}</{optionalField.Key}>\n");
      }
      writer.Write("    <dependencies>\n");
      foreach (var packageDependency in packageDependencies) {
        writer.Write($"      <dependency id=\"{packageDependency.PackageId}\" version=\"{packageDependency.Version}\" />\n");
      }
      writer.Write("    </dependencies>\n");
      writer.Write("  </metadata>\n");
      writer.Write("  <files>\n");
      foreach (var file in files) {
        writer.Write($"    <file src=\"{file.FileToPackage}\" target=\"{file.PathInPackage}\" />\n");
      }
      writer.Write("  </files>\n");
      writer.Write("</package>");
    }
  }
}