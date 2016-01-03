using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bud.Util;
using Microsoft.Win32;
using static System.IO.File;
using static System.IO.Path;
using static Bud.Util.Optional;

namespace Bud.NuGet {
  public class WindowsFrameworkAssemblyResolver {
    public static readonly string OldFrameworkPath
      = Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                "Microsoft.NET", "Framework");

    public static readonly string Net3PlusFrameworkPath
      = Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                "Reference Assemblies", "Microsoft", "Framework");

    public static readonly string Net4PlusFrameworkPath
      = Combine(Net3PlusFrameworkPath, ".NETFramework");

    public static Optional<string> ResolveFrameworkAssembly(string assemblyName, Version version) {
      var assembly = None<string>();
      if (version.Major == 0) {
        version = new Version(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);
      }
      if (version.Major >= 4) {
        assembly = FindNet4PlusAssembly(assemblyName, version);
      }
      if (version >= new Version(3, 5) && !assembly.HasValue) {
        assembly = FindNet3Assembly(5, assemblyName);
      }
      if (version >= new Version(3, 0) && !assembly.HasValue) {
        assembly = FindNet3Assembly(0, assemblyName);
      }
      if (version >= new Version(2, 0) && !assembly.HasValue) {
        assembly = FindNet2Assembly(assemblyName);
      }
      return assembly.HasValue ?
               assembly :
               AssemblyFoldersEx.FindAssembly(assemblyName, version);
    }

    private static Optional<string> FindNet4PlusAssembly(string assemblyName, Version version) {
      var rootReferenceAssemblyPath = ToNet4RawAssemblyPath(assemblyName,
                                                            version);
      if (Exists(rootReferenceAssemblyPath)) {
        return rootReferenceAssemblyPath;
      }
      var facadeReferenceAssemblyPath = ToFacadePath(assemblyName, version);
      if (Exists(facadeReferenceAssemblyPath)) {
        return facadeReferenceAssemblyPath;
      }
      return None<string>();
    }

    private static string ToNet4RawAssemblyPath(string assemblyName,
                                                Version version)
      => Combine(Net4PlusFrameworkPath,
                 ToNet4PlusDir(version),
                 $"{assemblyName}.dll");

    private static string ToFacadePath(string assemblyName,
                                       Version version)
      => Combine(Net4PlusFrameworkPath,
                 ToNet4PlusDir(version),
                 "Facades",
                 $"{assemblyName}.dll");

    private static string ToNet4PlusDir(Version version)
      => version.Build == 0 ?
           $"v{version.Major}.{version.Minor}" :
           $"v{version.Major}.{version.Minor}.{version.Build}";

    private static Optional<string> FindNet3Assembly(int minorVersion,
                                                     string assemblyName) {
      var assembly = Combine(Net3PlusFrameworkPath,
                             ToNet3Dir(minorVersion),
                             $"{assemblyName}.dll");
      if (Exists(assembly)) {
        return assembly;
      }
      assembly = Combine(OldFrameworkPath,
                         ToNet3Dir(minorVersion),
                         $"{assemblyName}.dll");
      return Exists(assembly) ? assembly : None<string>();
    }

    private static string ToNet3Dir(int minorVersion)
      => $"v3.{minorVersion}";

    private static Optional<string> FindNet2Assembly(string assemblyName) {
      var assembly = Combine(OldFrameworkPath,
                             "v2.0.50727",
                             $"{assemblyName}.dll");
      return Exists(assembly) ? assembly : None<string>();
    }

    private static class AssemblyFoldersEx {
      private static readonly ImmutableSortedDictionary<Version, IImmutableSet<string>>
        VersionToFoldersMap = GetAssemblyFoldersEx();

      public static Optional<string> FindAssembly(string assemblyName,
                                                  Version version) {
        foreach (var versionFolders in VersionToFoldersMap) {
          if (versionFolders.Key >= version) {
            foreach (var folder in versionFolders.Value) {
              var assemblyPath = Combine(folder, $"{assemblyName}.dll");
              if (Exists(assemblyPath)) {
                return assemblyPath;
              }
            }
          } else {
            break;
          }
        }
        return None<string>();
      }

      private static ImmutableSortedDictionary<Version, IImmutableSet<string>>
        GetAssemblyFoldersEx() {
        var netFrameworkRegKey = Registry
          .LocalMachine
          .OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\.NETFramework");
        var versions = netFrameworkRegKey
          .GetSubKeyNames()
          .Where(key => key.StartsWith("v")).ToImmutableSortedDictionary(
            versionKey => Version.Parse(versionKey.Substring(1)),
            versionKey => CollectAssemblyFolders(netFrameworkRegKey, versionKey),
            new VersionComparerReverse());
        return versions;
      }

      private static IImmutableSet<string>
        CollectAssemblyFolders(RegistryKey netFrameworkRegKey,
                               string verionKey) {
        var assemblyFoldersKey = netFrameworkRegKey
          .OpenSubKey($@"{verionKey}\AssemblyFoldersEx");
        if (assemblyFoldersKey == null) {
          return ImmutableHashSet<string>.Empty;
        }
        return assemblyFoldersKey
          .GetSubKeyNames()
          .Select(folderKey => assemblyFoldersKey.OpenSubKey(folderKey)?
                                                 .GetValue(null) as string)
          .Where(folder => !string.IsNullOrEmpty(folder))
          .ToImmutableHashSet();
      }

      private class VersionComparerReverse : IComparer<Version> {
        public int Compare(Version x, Version y) => y.CompareTo(x);
      }
    }
  }
}