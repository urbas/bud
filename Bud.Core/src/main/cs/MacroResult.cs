namespace Bud {
  public class MacroResult {
    public MacroResult(object value, BuildContext buildContext) {
      BuildContext = buildContext;
      Value = value;
    }

    public BuildContext BuildContext { get; }

    public object Value { get; }
  }
}