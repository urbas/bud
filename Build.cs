using Bud.V1;
using static Bud.V1.Api;
using static Microsoft.CodeAnalysis.OutputKind;

public class BudBuild : IBuild {
  public Conf Init()
    => Projects(CsLibrary("Bud")
                  .SetValue(Api.Version, "0.5.0-alpha-1")
                  .Modify(CsCompilationOptions, (c, oldValue) => oldValue.WithOutputKind(ConsoleApplication)),
                CsLibrary("Bud.Test")
                  .Add(Dependencies, "../Bud"));
}