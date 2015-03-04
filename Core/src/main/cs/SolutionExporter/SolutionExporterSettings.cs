using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Antlr4.StringTemplate;
using Bud.Build;
using Bud.CSharp;
using Bud.Dependencies;
using NuGet;

namespace Bud.SolutionExporter {
  public static class SolutionExporterSettings {
    public static Settings GenerateSolution(this Settings settings) {
      return settings.Globally(SolutionExporterKeys.GenerateSolution.Init(GenerateSolutionImpl));
    }

    private static async Task GenerateSolutionImpl(IContext context) {
      using (var manifestResourceStream = typeof (SolutionExporterSettings).Assembly.GetManifestResourceStream("Bud.CsProjectTemplate.csproj")) {
        var csProjectTemplate = manifestResourceStream.ReadToEnd();
        try {
          await GenerateCsprojs(context, csProjectTemplate);
        } catch (Exception e) {
          Console.WriteLine(e.ToString());
        }
      }
    }

    private static async Task GenerateCsprojs(IContext context, string csProjectTemplate) {
      foreach (var buildTarget in BuildTargetUtils.GetAllBuildTargets(context)) {
        var buildTargetCsprojFile = GetBuildTargetCsprojPath(context, buildTarget);
        context.Logger.Info(string.Format("Generating '{0}'...", buildTargetCsprojFile));
        var template = new Template(csProjectTemplate, '%', '%');
        var csprojUri = new Uri(buildTargetCsprojFile);
        var sourceFiles = await CollectSourceFiles(context, buildTarget, csprojUri);
        template.Add("assemblyName", context.GetCSharpOutputAssemblyName(buildTarget));
        template.Add("outputType", context.GetCSharpAssemblyType(buildTarget));
        template.Add("assemblyReferences", CollectAssemblyReferences(context, buildTarget, csprojUri));
        template.Add("projectReferences", CollectProjectReferences(context, buildTarget, csprojUri));
        template.Add("projectGuid", BuildTargetUtils.GuidOf(buildTarget));
        template.Add("builtTargetScope", BuildTargetUtils.ScopeOf(buildTarget).Id);
        template.Add("sourceFiles", sourceFiles);
        template.Add("rootNamespace", context.GetRootNamespace(buildTarget));
        using (var fileStream = new FileStream(buildTargetCsprojFile, FileMode.Create, FileAccess.Write)) {
          using (var streamWriter = new StreamWriter(fileStream)) {
            template.Write(new AutoIndentWriter(streamWriter));
          }
        }
      }
    }

    private static async Task<IEnumerable> CollectSourceFiles(IContext context, Key buildTarget, Uri csprojUri) {
      var sourceFiles = await context.GetCSharpSources(buildTarget);
      return sourceFiles.Select(path => new {
        AbsolutePath = path,
        RelativePath = csprojUri.MakeRelativeUri(new Uri(path))
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
                      Guid = BuildTargetUtils.GuidOf(ar.BuildTarget)
                    });
    }

    private static string GetBuildTargetCsprojPath(IContext context, Key buildTarget) {
      return Path.Combine(context.GetBaseDir(buildTarget),
                          context.GetCSharpOutputAssemblyName(buildTarget) + ".tmp.csproj");
    }
  }
}