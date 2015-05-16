using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bud.Build;
using Bud.BuildDefinition;
using Bud.CSharp;
using Bud.Dependencies;
using Bud.Projects;
using Bud.Util;
using NuGet;

namespace Bud.Publishing {
  public class PublishingPlugin : Plugin {
    public readonly static PublishingPlugin Instance = new PublishingPlugin();

    public override Settings Setup(Settings settings) {
      return settings.Add(PublishKeys.Publish.Init(PublishImpl),
                          PublishKeys.Package.Init(PackageImpl))
                     .AddGlobally(PublishKeys.Publish.Init(TaskUtils.NoOpTask),
                                  PublishKeys.Publish.DependsOn(settings.Scope / PublishKeys.Publish),
                                  PublishKeys.PublishApiKey.Init(PublishApiKeyImpl));
    }

    private static async Task<string> PackageImpl(IContext context, Key buildTarget) {
      context.Logger.Info("preparing package...");
      await context.Evaluate(buildTarget / BuildKeys.Build);
      var project = BuildTargetUtils.ProjectOf(buildTarget);
      var buildTargetId = context.PackageIdOf(buildTarget);
      var targetFramework = context.GetTargetFramework(buildTarget);
      var packageDir = Path.Combine(context.GetOutputDir(project), "package");
      var packageFile = Path.Combine(packageDir, buildTargetId + ".nupkg");
      Directory.CreateDirectory(packageDir);
      var projectUrl = new Uri($"https://www.nuget.org/packages/{buildTargetId}/");
      var packageBuilder = new PackageBuilder {
        Id = buildTargetId,
        Version = context.GetVersionOf(project),
        Title = buildTargetId,
        LicenseUrl = projectUrl,
        ProjectUrl = projectUrl,
        IconUrl = projectUrl,
        RequireLicenseAcceptance = false,
        DevelopmentDependency = false,
        Description = project.Id,
        Copyright = "",
        Summary = "",
        ReleaseNotes = "",
        Language = ""
      };

      packageBuilder.Authors.Add(Environment.UserName);
      packageBuilder.Owners.Add(Environment.UserName);
      packageBuilder.Tags.Add(buildTargetId);
      packageBuilder.DependencySets.Add(new PackageDependencySet(
                                          targetFramework.FrameworkName,
                                          context.GetDependencies(buildTarget)
                                                 .Select(d => d.AsPackage(context))
                                                 .Select(p => new PackageDependency(p.Id, VersionUtility.GetSafeRange(p.Version))).ToList()));
      packageBuilder.Files.Add(new CSharpBuildTargetAssembly(context, buildTarget).ToPackagedAssembly());

      try {
        using (var nupkgFileStream = new FileStream(packageFile, FileMode.Create)) {
          packageBuilder.Save(nupkgFileStream);
        }
      } catch (Exception e) {
        context.Logger.Error("Failed to create the package: " + e);
      }
      return packageFile;
    }

    private static Task PublishImpl(IContext context, Key buildTarget) {
      return Task.Run(async () => {
        try {
          context.Logger.Info("publishing...");
          string packageFile = await PublishTasks.Package(context, buildTarget);
          var packageServer = new PackageServer("https://www.nuget.org", "IE");
          packageServer.PushPackage(context.Evaluate(PublishKeys.PublishApiKey), new ZipPackage(packageFile), new FileInfo(packageFile).Length, 3600000, false);
        } catch (Exception e) {
          context.Logger.Error("publishing failed: " + e);
        }
      });
    }

    private static string PublishApiKeyImpl(IConfig config) {
      var publishApiKeyFile = Path.Combine(BudPaths.GlobalConfigDir, "nuGetApiKey");
      if (File.Exists(publishApiKeyFile)) {
        return File.ReadAllText(publishApiKeyFile).Trim();
      }
      var message = $"Could not find the NuGet publish API key. Please place the API key into the file '{publishApiKeyFile}'.";
      config.Logger.Error(message);
      throw new Exception(message);
    }
  }
}