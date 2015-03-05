using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.IO.Path;
using System.Linq;
using System.Threading.Tasks;
using Antlr4.StringTemplate;
using Bud.Build;
using Bud.Build.BuildTargetUtils;
using Bud.CSharp;
using Bud.Dependencies;
using Bud.IO.Paths;
using Bud.Resources;
using NuGet;

namespace Bud.SolutionExporter {
  public static class SolutionExporterSettings {
    public static Settings GenerateSolution(this Settings settings) {
      return settings.Globally(SolutionExporterKeys.GenerateSolution.Init(GenerateSolutionImpl));
    }

    private static async Task GenerateSolutionImpl(IContext context) {
      GenerateSolution(context);
      await GenerateCsprojs(context);
    }

    private static void GenerateSolution(IContext context) {
      using (var solutionTemplateStream = typeof(SolutionExporterSettings).Assembly.GetManifestResourceStream("Bud.SolutionTemplate.sln")) {
        try {
          var generatedSolutionPath = Combine(context.GetBaseDir(), GetFileName(context.GetBaseDir()) + ".tmp.sln");
          context.Logger.Info(string.Format("Generating '{0}'...", generatedSolutionPath));
          var template = new Template(solutionTemplateStream.ReadToEnd(), '%', '%');
          var generatedSolutionPathUri = new Uri(generatedSolutionPath);
          template.Add("projects", GetCSharpBuildTargets(context).Select(buildTarget => new {
            BuildTarget = buildTarget,
            Guid = GuidOf(buildTarget),
            Id = PackageIdOf(buildTarget),
            RelativeCsprojPath = generatedSolutionPathUri.MakeRelativeUri(new Uri(GetBuildTargetCsprojPath(context, buildTarget)))
          }));
          RenderTemplate(generatedSolutionPath, template);
        } catch (Exception e) {
          // TODO: The exceptions thrown by StringTemplate are not serializable. Therefore, they cannot cross app domain boundaries. We have to decide how to handle app domain boundaries. Maybe encode everything with JSON.
          Console.WriteLine(e.ToString());
        }
      }
    }

    private static async Task GenerateCsprojs(IContext context) {
      using (var csprojTemplateStream = typeof(SolutionExporterSettings).Assembly.GetManifestResourceStream("Bud.CsProjectTemplate.csproj")) {
        try {
          var csprojTemplateAsString = csprojTemplateStream.ReadToEnd();
          foreach (var buildTarget in GetCSharpBuildTargets(context)) {
            var buildTargetCsprojFile = GetBuildTargetCsprojPath(context, buildTarget);
            context.Logger.Info(string.Format("Generating '{0}'...", buildTargetCsprojFile));
            var template = new Template(csprojTemplateAsString, '%', '%');
            var csprojUri = new Uri(buildTargetCsprojFile);
            var sourceFiles = await CollectSourceFiles(context, buildTarget, csprojUri);
            var embeddedResourceFiles = await CollectEmbeddedResourceFiles(context, buildTarget, csprojUri);
            template.Add("assemblyName", context.GetCSharpOutputAssemblyName(buildTarget));
            template.Add("outputType", context.GetCSharpAssemblyType(buildTarget));
            template.Add("assemblyReferences", CollectAssemblyReferences(context, buildTarget, csprojUri));
            template.Add("projectReferences", CollectProjectReferences(context, buildTarget, csprojUri));
            template.Add("projectGuid", GuidOf(buildTarget));
            template.Add("builtTargetScope", ScopeOf(buildTarget).Id);
            template.Add("sourceFiles", sourceFiles);
            template.Add("embeddedResources", embeddedResourceFiles);
            template.Add("rootNamespace", context.GetRootNamespace(buildTarget));
            RenderTemplate(buildTargetCsprojFile, template);
          }
        } catch (Exception e) {
          // TODO: The exceptions thrown by StringTemplate are not serializable. Therefore, they cannot cross app domain boundaries. We have to decide how to handle app domain boundaries. Maybe encode everything with JSON.
          Console.WriteLine(e.ToString());
        }
      }
    }

    private static IEnumerable<Key> GetCSharpBuildTargets(IContext context) {
      return GetAllBuildTargets(context).Where(buildTarget => buildTarget.Leaf.Equals(CSharpKeys.CSharp));
    }

    private static void RenderTemplate(string outputFilePath, Template template) {
      using (var fileStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write)) {
        using (var streamWriter = new StreamWriter(fileStream)) {
          template.Write(new AutoIndentWriter(streamWriter));
        }
      }
    }

    private static async Task<IEnumerable> CollectSourceFiles(IContext context, Key buildTarget, Uri csprojUri) {
      var sourceFiles = await context.GetSourceFiles(buildTarget);
      return ToRelativePaths(csprojUri, sourceFiles);
    }

    private static async Task<IEnumerable> CollectEmbeddedResourceFiles(IContext context, Key buildTarget, Uri csprojUri) {
      var resourcesBuildTarget = buildTarget.Parent / ResourcesKeys.Resources;
      if (context.IsBuildTargetDefined(resourcesBuildTarget)) {
        var embeddedResourceFiles = await context.GetSourceFiles(resourcesBuildTarget);
        return ToRelativePaths(csprojUri, embeddedResourceFiles);
      }
      return ImmutableList<object>.Empty;
    }

    private static IEnumerable ToRelativePaths(Uri csprojUri, IEnumerable<string> sourceFiles) {
      return sourceFiles.Select(path => new {
        AbsolutePath = path,
        RelativePath = ToWindowsPath(csprojUri.MakeRelativeUri(new Uri(path)).ToString())
      });
    }

    private static IEnumerable CollectAssemblyReferences(IContext context, Key buildTarget, Uri csprojUri) {
      return context.GetAssemblyReferences(buildTarget)
                    .OfType<AssemblyReference>()
                    .Select(ar => new {
                      Reference = ar,
                      RelativePath = csprojUri.MakeRelativeUri(new Uri(ar.Path))
                    });
    }

    private static IEnumerable CollectProjectReferences(IContext context, Key buildTarget, Uri csprojUri) {
      return context.GetAssemblyReferences(buildTarget)
                    .OfType<CSharpBuildTargetAssembly>()
                    .Select(ar => new {
                      Reference = ar,
                      RelativePath = csprojUri.MakeRelativeUri(new Uri(GetBuildTargetCsprojPath(context, ar.BuildTarget))),
                      Guid = GuidOf(ar.BuildTarget)
                    });
    }

    private static string GetBuildTargetCsprojPath(IContext context, Key buildTarget) {
      return Combine(context.GetBaseDir(buildTarget),
                     context.GetCSharpOutputAssemblyName(buildTarget) + ".tmp.csproj");
    }
  }
}