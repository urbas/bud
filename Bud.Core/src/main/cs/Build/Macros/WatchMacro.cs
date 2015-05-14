using System.Linq;
using CommandLine;

namespace Bud.Build.Macros {
  internal static class WatchMacro {
    public const string WatchMacroDescription = "Watches sources and triggers tasks when a source file changes.";

    public static MacroResult WatchMacroImpl(IBuildContext context, string[] commandLineArgs) {
      var cliArguments = new WatchMacroArguments();
      if (Parser.Default.ParseArguments(commandLineArgs, cliArguments)) {
        var watchedDirs = context.Config.GetAllBuildTargets().Select(context.Config.GetBaseDir);
        var watchingTaskEvaluator = new WatchingTaskEvaluator(watchedDirs, cliArguments.WatchedTasks);
        watchingTaskEvaluator.StartWatching(context.Context);
      }
      return new MacroResult(null, context);
    }
  }
}