using System;
using System.Collections.Immutable;
using System.Linq;

namespace Bud.Configuration {
  public class ConfAccessException : Exception {
    public IImmutableList<string> ReferencePath { get; }

    public ConfAccessException(IImmutableList<string> referencePath,
                               Exception cause)
      : base(string.Empty, cause) {
      ReferencePath = referencePath;
    }

    public override string Message
      => $"Failed calculating value of key {ReferencePathString()}.";

    private string ReferencePathString()
      => string.Join(" referenced from ", ReferencePath.Select(p => $"'{p}'"));
  }
}