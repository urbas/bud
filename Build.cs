using Bud.V1;
using static Bud.V1.Api;

public class BudBuild : IBuild {
  public Conf Init()
    => Projects(CsApp("bud")
                  .SetValue(ProjectVersion, "0.5.0-pre-4")
                  .SetValue(ProjectUrl, "https://github.com/urbas/bud"),
                CsLib("Bud.Test")
                  .Add(Dependencies, "../bud"));
}