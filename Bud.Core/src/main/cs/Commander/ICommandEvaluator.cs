namespace Bud.Commander {
  public interface ICommandEvaluator {
    string EvaluateToJsonSync(string command, ref IBuildContext context);
    string EvaluateMacroToJsonSync(string macroName, string[] commandLineParameters, ref IBuildContext context);
  }
}