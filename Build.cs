using Bud.V1;
using static Bud.V1.Api;

public class BudBuild : IBuild {
  public Conf Init()
    => Projects(CsApp("Bud")
                  .SetValue(Api.Version, "0.5.0-alpha-1"),
                CsLibrary("Bud.Test")
                  .Add(Dependencies, "../Bud"));
}