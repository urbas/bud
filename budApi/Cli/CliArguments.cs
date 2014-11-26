using System;
using System.Text;
using System.Collections.Generic;

namespace Bud
{
	public class CliArguments
  {
    private readonly StringBuilder arguments = new StringBuilder();

    public CliArguments Add(string argument) {
      if (arguments.Length > 0) {
        arguments.Append(' ');
      }
      arguments.Append('"').Append(argument).Append('"');
      return this;
    }

    public CliArguments Add(IEnumerable<string> arguments) {
      foreach (var argument in arguments) {
        Add(argument);
      }
      return this;
    }

    public override string ToString() {
      return arguments.ToString();
    }
    
	}
}

