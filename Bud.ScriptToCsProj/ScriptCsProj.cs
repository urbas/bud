using System.Collections.Generic;
using System.Linq;
using Bud.CsProjTools;
using Bud.Scripting;

namespace Bud.ScriptToCsProj {
  public class ScriptCsProj {
    public static string BudScriptCsProj(IEnumerable<Reference> references)
      => CsProj.Generate(
        CsProj.Import(@"$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props", @"Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')"),
        CsProj.PropertyGroup(CsProj.Property("Configuration", "Debug", @" '$(Configuration)' == '' "),
                             CsProj.Property("Platform", "AnyCPU", @" '$(Platform)' == '' "),
                             CsProj.Property("OutputType", "Exe"),
                             CsProj.Property("RootNamespace", ""),
                             CsProj.Property("AssemblyName", "script"),
                             CsProj.Property("TargetFrameworkVersion", "v4.6")),
        CsProj.PropertyGroup(@" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' ",
                             CsProj.Property("PlatformTarget", "AnyCPU"),
                             CsProj.Property("DebugSymbols", "true"),
                             CsProj.Property("DebugType", "full"),
                             CsProj.Property("Optimize", "false"),
                             CsProj.Property("OutputPath", @"bin\Debug\"),
                             CsProj.Property("DefineConstants", @"DEBUG;TRACE"),
                             CsProj.Property("ErrorReport", @"prompt"),
                             CsProj.Property("WarningLevel", "4")),
        CsProj.ItemGroup(references.Select(r => CsProj.Reference(r.AssemblyName, r.Path)).ToArray()),
        CsProj.ItemGroup(CsProj.Item("Compile", "Build.cs")),
        CsProj.Import(@"$(MSBuildToolsPath)\Microsoft.CSharp.targets"));
  }
}