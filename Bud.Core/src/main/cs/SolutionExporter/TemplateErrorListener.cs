using System;
using Antlr4.StringTemplate;
using Antlr4.StringTemplate.Misc;

namespace Bud.SolutionExporter {
  public class TemplateErrorListener : ITemplateErrorListener {
    public void CompiletimeError(TemplateMessage msg) {
      throw new Exception(msg.ToString());
    }

    public void RuntimeError(TemplateMessage msg) {
      throw new Exception(msg.ToString());
    }

    public void IOError(TemplateMessage msg) {
      throw new Exception(msg.ToString());
    }

    public void InternalError(TemplateMessage msg) {
      throw new Exception(msg.ToString());
    }
  }
}