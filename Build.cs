using Bud.V1;
using static Bud.V1.Api;

public class BudBuild : IBuild {
  public Conf Init()
    => Projects(CsApp("Bud")
                  .SetValue(ProjectVersion, "0.5.0-alpha-1")
                  .SetValue(PublishUrl, "https://www.nuget.org")
                  .Init("/dist-zip", c => DistributionArchive[c]),
                CsLibrary("Bud.Test")
                  .Add(Dependencies, "../Bud"));
}