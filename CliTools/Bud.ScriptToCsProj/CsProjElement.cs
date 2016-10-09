using System;
using System.Collections.Generic;
using System.Linq;

namespace Bud.ScriptToCsProj {
  public class CsProjElement {
    public string Name { get; }
    public Option<string> Content { get; }
    public IEnumerable<CsProjElement> Children { get; }
    public IEnumerable<Tuple<string, string>> Attributes { get; }

    public CsProjElement(string name,
                         IEnumerable<Tuple<string, string>> attributes,
                         IEnumerable<CsProjElement> children,
                         Option<string> content = default(Option<string>)) {
      Attributes = attributes.ToList();
      Name = name;
      Children = children;
      Content = content;
    }
  }
}