using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Bud.Make {
  public class Rules : IEnumerable<Rule> {
    private readonly IImmutableList<Rule> rules;

    public Rules(IImmutableList<Rule> rules) {
      this.rules = rules;
    }

    public IEnumerator<Rule> GetEnumerator() => rules.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public Rules Add(Rule rule) => new Rules(rules.Add(rule));
  }
}