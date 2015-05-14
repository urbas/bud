namespace Bud {
  public class MacroResult {
    public MacroResult(object value, IBuildContext buildContext) {
      BuildContext = buildContext;
      Value = value;
    }

    public IBuildContext BuildContext { get; }

    public object Value { get; }
  }
}