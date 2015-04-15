using System;
using System.Collections.Generic;
using System.Runtime.Versioning;

namespace Bud.CSharp {
  public class Framework {
    public static readonly Framework Net20 = new Framework("net20", @"C:\Windows\Microsoft.NET\Framework\v2.0.50727\csc.exe", "/usr/bin/mcs", new string[0], new string[0], new FrameworkName(".NETFramework,Version=v2.0"));
    public static readonly Framework Net45 = new Framework("net45", @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe", "/usr/bin/mcs", new[] {"System.Runtime.dll"}, new[] {"Facades/System.Runtime.dll"}, new FrameworkName(".NETFramework,Version=v4.5"));
    public static readonly Framework Net46 = new Framework("net46", @"C:\Program Files (x86)\MSBuild\14.0\Bin\csc2.exe", "/usr/bin/mcs", new[] {"System.Runtime.dll"}, new[] {"Facades/System.Runtime.dll"}, new FrameworkName(".NETFramework,Version=v4.6"));

    private readonly string WindowsCSharpCompilerPath;
    private readonly string LinuxCSharpCompilerPath;
    private readonly string[] WindowsSystemRuntimeFacadeDll;
    private readonly string[] LinuxSystemRuntimeFacadeDll;

    private Framework(string identifier, string windowsCSharpCompilerPath, string linuxCSharpCompilerPath, string[] windowsSystemRuntimeFacadeDll, string[] linuxSystemRuntimeFacadeDll, FrameworkName frameworkName) {
      Identifier = identifier;
      FrameworkName = frameworkName;
      WindowsCSharpCompilerPath = windowsCSharpCompilerPath;
      LinuxCSharpCompilerPath = linuxCSharpCompilerPath;
      WindowsSystemRuntimeFacadeDll = windowsSystemRuntimeFacadeDll;
      LinuxSystemRuntimeFacadeDll = linuxSystemRuntimeFacadeDll;
    }

    public string CSharpCompilerPath => Environment.OSVersion.Platform == PlatformID.Unix ? LinuxCSharpCompilerPath : WindowsCSharpCompilerPath;

    public IEnumerable<string> RuntimeAssemblies => Environment.OSVersion.Platform == PlatformID.Unix ? LinuxSystemRuntimeFacadeDll : WindowsSystemRuntimeFacadeDll;

    public FrameworkName FrameworkName { get; private set; }

    public string Identifier { get; private set; }
  }
}