using Bud.V1;
using static Bud.V1.Api;

public class BudBuild : IBuild {
  public Conf Init()
    => Projects(CsLibrary("Bud")
                  .SetValue(PublishUrl, "file://C:/Users/matej/Programming/NuGetLocalRepo"),
                CsLibrary("Bud.Test")
                  .Add(Dependencies, "../Bud"));
}