using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using Bud.V1;
using static System.Environment;
using static System.Environment.SpecialFolder;
using static System.IO.Path;
using static Bud.V1.Api;
using static Microsoft.CodeAnalysis.OutputKind;

public class BudBuild : IBuild {
  public Conf Init()
    => Projects(CsLibrary("Bud")
                  .SetValue(Api.Version, "0.0.1-alpha-1")
                  .SetValue(PublishUrl, "file://C:/Users/matej/Programming/NuGetLocalRepo")
                  .Modify(CsCompilationOptions, (c, oldValue) => oldValue.WithOutputKind(ConsoleApplication))
                  .Init("/install", Install),
                CsLibrary("Bud.Test")
                  .Add(Dependencies, "../Bud"));

  private static IObservable<bool> Install(IConf c) {
    var installDir = CreateBudInstallDir();
    return Compile[c].Select(output => CopyFiles(new[] {output.AssemblyPath.ToLowerInvariant()}, installDir))
                     .CombineLatest(CopyAssemblies(c, installDir),
                                    PrintInstallSuccess);
  }

  private static string CreateBudInstallDir() {
    var installDir = Combine(GetFolderPath(LocalApplicationData), "Bud", "bin");
    Directory.CreateDirectory(installDir);
    return installDir;
  }

  private static bool CopyFiles(IEnumerable<string> assemblies, string installDir) {
    foreach (var assembly in assemblies) {
      File.Copy(assembly, Combine(installDir, GetFileName(assembly)), true);
    }
    return true;
  }

  private static IObservable<bool> CopyAssemblies(IConf c, string installDir)
    => AssemblyReferences[c].Select(assemblies => CopyFiles(assemblies, installDir));

  private static bool PrintInstallSuccess(bool successBud, bool successAssemblies) {
    Console.WriteLine("Installed successfully.");
    return successBud && successAssemblies;
  }
}