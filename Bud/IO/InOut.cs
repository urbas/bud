using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bud.Collections;

namespace Bud.IO {
  public class InOut {
    public static readonly InOut Empty = new InOut(ImmutableList<object>.Empty);

    public InOut(params object[] elements) : this(elements.ToImmutableList()) {}

    public InOut(IEnumerable<object> elements) : this(elements.ToImmutableList()) {}

    public InOut(IImmutableList<object> elements) {
      Elements = elements;
    }

    public IImmutableList<object> Elements { get; }

    public InOut Add(IEnumerable<object> ioElements)
      => new InOut(Elements.AddRange(ioElements));

    public static InOut Merge(params InOut[] inOuts) => Merge((IEnumerable<InOut>) inOuts);

    public static InOut Merge(IEnumerable<InOut> inOuts)
      => new InOut(inOuts.Aggregate(ImmutableList.CreateBuilder<object>(), (builder, inOut) => {
        builder.AddRange(inOut.Elements);
        return builder;
      }).ToImmutable());

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) {
        return false;
      }
      if (ReferenceEquals(this, obj)) {
        return true;
      }
      return obj.GetType() == GetType() && Equals((InOut) obj);
    }

    protected bool Equals(InOut other) => Elements.SequenceEqual(other.Elements);
    public override int GetHashCode() => EnumerableUtils.ElementwiseHashCode(Elements);
    public static bool operator ==(InOut left, InOut right) => Equals(left, right);
    public static bool operator !=(InOut left, InOut right) => !Equals(left, right);
    public override string ToString() => $"InOut({String.Join(", ", Elements)})";
    public static InOut ToInOut(object io) => new InOut(io);
  }
}