using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reactive.Linq;
using System.Text;
using Bud.V1;
using static Bud.V1.Api;

public class BudBuild : IBuild {
  public Conf Init()
    => Projects(CsApp("Bud")
                  .SetValue(ProjectVersion, "0.5.0-pre-1"),
                CsLibrary("Bud.Test")
                  .Add(Dependencies, "../Bud"));
}