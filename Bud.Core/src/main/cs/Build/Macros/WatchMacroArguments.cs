using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace Bud.Build.Macros {
  internal class WatchMacroArguments {
    [ValueList(typeof(List<string>))]
    public List<string> WatchedTasks { get; set; }

    [HelpOption]
    public string GetUsage() {
      var helpText = HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
      helpText.Heading = new HeadingInfo("Bud: watch macro");
      helpText.Copyright = new CopyrightInfo("Matej Urbas", 2015);
      helpText.AddPreOptionsLine("\n" + WatchMacro.WatchMacroDescription);
      helpText.AddPreOptionsLine("\nOptions:");
      return helpText;
    }
  }
}