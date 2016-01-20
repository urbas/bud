using System.Reactive;
using Bud.V1;
using static Bud.V1.Api;

public class BudBuild : IBuild {
  public Conf Init()
    => Projects(CsLibrary("Bud"),
                CsLibrary("Bud.Test")
                  .Add(Dependencies, "../Bud"),
                Clean.Init(c => {
                  c.TryGet("Bud"/Clean);
                  c.TryGet("Bud.Test"/Clean);
                  return Unit.Default;
                }));
}