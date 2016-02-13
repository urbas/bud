using Bud.V1;
using static Bud.V1.Api;

public class BudBuild : IBuild {
  public Conf Init()
    => Projects(CsApp("Bud")
                  .SetValue(ProjectVersion, "0.5.0-pre-1"),
                CsLibrary("Bud.Test")
                  .Add(Dependencies, "../Bud"));
}