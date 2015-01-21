using System;
using System.Collections.Generic;

namespace Bud.Plugins.CSharp {
  public class Framework {
    public static readonly Framework Net20 = new Framework(@"C:\Windows\Microsoft.NET\Framework\v2.0.50727\csc.exe", "/usr/bin/mcs", new string[0], new string[0]);
    public static readonly Framework Net45 = new Framework(@"C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe", "/usr/bin/mcs", new[] { "System.Runtime.dll" }, new[] { "Facades/System.Runtime.dll" });

    private readonly string windowsCSharpCompilerPath;
    private readonly string linuxCSharpCompilerPath;
    private readonly string[] windowsSystemRuntimeFacadeDll;
    private readonly string[] linuxSystemRuntimeFacadeDll;

    private Framework(string windowsCSharpCompilerPath, string linuxCSharpCompilerPath, string[] windowsSystemRuntimeFacadeDll, string[] linuxSystemRuntimeFacadeDll) {
      this.windowsCSharpCompilerPath = windowsCSharpCompilerPath;
      this.linuxCSharpCompilerPath = linuxCSharpCompilerPath;
      this.windowsSystemRuntimeFacadeDll = windowsSystemRuntimeFacadeDll;
      this.linuxSystemRuntimeFacadeDll = linuxSystemRuntimeFacadeDll;
    }

    public string CSharpCompilerPath {
      get {
        if (Environment.OSVersion.Platform == PlatformID.Unix) {
          return linuxCSharpCompilerPath;
        }
        return windowsCSharpCompilerPath;
      }
    }

    public IEnumerable<string> RuntimeAssemblies {
      get {
        if (Environment.OSVersion.Platform == PlatformID.Unix) {
          return linuxSystemRuntimeFacadeDll;
        }
        return windowsSystemRuntimeFacadeDll;
      }
    }
  }
}