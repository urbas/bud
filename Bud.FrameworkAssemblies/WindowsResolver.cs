using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using static System.Environment;
using static System.IO.Path;
using Version = System.Version;

namespace Bud.FrameworkAssemblies {
  public class WindowsResolver {
    public static readonly string OldFrameworkPath
      = Combine(GetFolderPath(SpecialFolder.Windows),
                "Microsoft.NET", "Framework");

    public static readonly string Net3PlusFrameworkPath
      = Combine(GetFolderPath(SpecialFolder.ProgramFilesX86),
                "Reference Assemblies", "Microsoft", "Framework");

    public static readonly string Net4PlusFrameworkPath
      = Combine(Net3PlusFrameworkPath, ".NETFramework");

    public static Option<string> ResolveFrameworkAssembly(string assemblyName, Version version) {
      if (version.Major == 0) {
        version = new Version(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);
      }
      var dllName = assemblyName + ".dll";
      var foundDll = FrameworkDirs
        .Where(frameworkDir => version >= frameworkDir.Version)
        .Select(frameworkDir => Combine(frameworkDir.Dir, dllName)).FirstOrDefault(File.Exists);
      return foundDll ?? Option.None<string>();
    }

    public static bool IsFrameworkAssembly(string dll)
      => FrameworkDirs.Any(f => File.Exists(Combine(f.Dir, dll)));

    public static ImmutableSortedSet<FrameworkDir> FrameworkDirs
      => FrameworkDirsLazy.FrameworkDirsCache;

    private static class FrameworkDirsLazy {
      public static readonly ImmutableSortedSet<FrameworkDir> FrameworkDirsCache = GetFrameworkDirs();

      private static ImmutableSortedSet<FrameworkDir> GetFrameworkDirs() {
        var net4PlusDirs = Directory.EnumerateDirectories(Net4PlusFrameworkPath, "v*", SearchOption.TopDirectoryOnly);
        var list = new List<FrameworkDir>();
        foreach (var dir in net4PlusDirs) {
          var facadesDir = Combine(dir, "Facades");
          var frameworkVersion = Version.Parse(GetFileName(dir).Substring(1));
          if (Directory.Exists(facadesDir)) {
            list.Add(new FrameworkDir(frameworkVersion, facadesDir));
          }
          list.Add(new FrameworkDir(frameworkVersion, dir));
        }
        list.Add(new FrameworkDir(new Version(3, 5), Combine(Net3PlusFrameworkPath, "v3.5")));
        list.Add(new FrameworkDir(new Version(3, 5), Combine(OldFrameworkPath, "v3.5")));
        list.Add(new FrameworkDir(new Version(3, 0), Combine(Net3PlusFrameworkPath, "v3.0")));
        list.Add(new FrameworkDir(new Version(3, 0), Combine(OldFrameworkPath, "v3.0")));
        list.Add(new FrameworkDir(new Version(2, 0), Combine(OldFrameworkPath, "v2.0.50727")));

        AddAssemblyFoldersEx(list);

        return list.ToImmutableSortedSet(new FrameworkDirComparer());
      }

      private static void AddAssemblyFoldersEx(ICollection<FrameworkDir> list) {
        using (var netFrameworkRegKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\.NETFramework")) {
          if (netFrameworkRegKey == null) {
            return;
          }
          foreach (var versionKey in netFrameworkRegKey
            .GetSubKeyNames()
            .Where(key => key.StartsWith("v"))) {
            CollectAssemblyFolders(netFrameworkRegKey, versionKey, list);
          }
        }
      }

      private static void CollectAssemblyFolders(RegistryKey netFrameworkRegKey,
                                                 string versionKey,
                                                 ICollection<FrameworkDir> list) {
        var rawVersion = Version.Parse(versionKey.Substring(1));
        var version = new Version(rawVersion.Major, rawVersion.Minor);
        using (var assemblyFoldersKey = netFrameworkRegKey.OpenSubKey($@"{versionKey}\AssemblyFoldersEx")) {
          if (assemblyFoldersKey == null) {
            return;
          }
          foreach (var dirKey in assemblyFoldersKey.GetSubKeyNames()) {
            using (var dirSubKey = assemblyFoldersKey.OpenSubKey(dirKey)) {
              var dir = dirSubKey?.GetValue(null) as string;
              if (!string.IsNullOrEmpty(dir)) {
                list.Add(new FrameworkDir(version, dir));
              }
            }
          }
        }
      }

      private class FrameworkDirComparer : IComparer<FrameworkDir> {
        public int Compare(FrameworkDir x, FrameworkDir y) {
          var versionComparison = y.Version.CompareTo(x.Version);
          if (versionComparison == 0) {
            return string.Compare(y.Dir, x.Dir, StringComparison.Ordinal);
          }
          return versionComparison;
        }
      }
    }
  }
}