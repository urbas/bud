using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reactive.Linq;
using Bud.V1;
using static System.IO.Path;
using static Bud.NuGet.WindowsFrameworkAssemblyResolver;
using static Bud.V1.Api;

public class BudBuild : IBuild {
  public Conf Init()
    => Projects(CsApp("Bud")
                  .SetValue(Api.Version, "0.5.0-alpha-1")
                  .SetValue(PublishUrl, "https://www.nuget.org")
                  .Init("/dist-zip", c => DistributionZip[c]),
                CsLibrary("Bud.Test")
                  .Add(Dependencies, "../Bud"));
}