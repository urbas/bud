using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bud.CsProjTools;
using Bud.Scripting;

namespace Bud.ScriptToCsProj {
  public class ScriptCsProj {
    /// <summary>
    ///   Generates a csproj file based on the script metadata. The csproj file will be
    ///   printed to the standard output.
    /// </summary>
    /// <param name="builtScriptMetadata">
    ///   metadata of the script for which to generate the
    ///   csproj file.
    /// </param>
    public static void OutputScriptCsProj(BuiltScriptMetadata builtScriptMetadata) {
      var customReferences = builtScriptMetadata.ResolvedScriptReferences
                                                .AssemblyReferences
                                                .Select(pair => new CsProjReference(pair.Key, pair.Value));
      var frameworkReferences = builtScriptMetadata.ResolvedScriptReferences
                                                   .FrameworkAssemblyReferences
                                                   .Select(pair => new CsProjReference(pair.Key));
      Console.Write(BudScriptCsProj(customReferences.Concat(frameworkReferences),
                                    Directory.GetCurrentDirectory()));
    }

    public static string BudScriptCsProj(IEnumerable<CsProjReference> references,
                                         string startWorkingDir)
      => CsProj.Generate(
        CsProj.Import(@"$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props", @"Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')"),
        CsProj.PropertyGroup(CsProj.Property("Configuration", "Debug", @" '$(Configuration)' == '' "),
                             CsProj.Property("Platform", "AnyCPU", @" '$(Platform)' == '' "),
                             CsProj.Property("OutputType", "Exe"),
                             CsProj.Property("RootNamespace", ""),
                             CsProj.Property("AssemblyName", "build-script"),
                             CsProj.Property("TargetFrameworkVersion", "v4.6")),
        CsProj.PropertyGroup(@" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' ",
                             CsProj.Property("PlatformTarget", "AnyCPU"),
                             CsProj.Property("DebugSymbols", "true"),
                             CsProj.Property("DebugType", "full"),
                             CsProj.Property("Optimize", "false"),
                             CsProj.Property("OutputPath", @"build\"),
                             CsProj.Property("DefineConstants", @"DEBUG;TRACE"),
                             CsProj.Property("ErrorReport", @"prompt"),
                             CsProj.Property("WarningLevel", "4")),
        CsProj.PropertyGroup(@"'$(Configuration)|$(Platform)' == 'Debug|AnyCPU'",
                             CsProj.Property("StartWorkingDirectory", startWorkingDir)),
        CsProj.ItemGroup(references.Select(r => CsProj.Reference(r.AssemblyName, r.Path)).ToArray()),
        CsProj.ItemGroup(CsProj.Item("Compile", "Build.cs")),
        CsProj.Import(@"$(MSBuildToolsPath)\Microsoft.CSharp.targets"));
  }
}