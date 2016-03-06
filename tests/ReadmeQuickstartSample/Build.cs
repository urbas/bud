using Bud.V1;
using static Bud.V1.Api;

public class BudBuild : IBuild {
  public Conf Init() => CsApp("Your.Project");
}