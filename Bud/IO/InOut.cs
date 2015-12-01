using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bud.Collections;

namespace Bud.IO {
  public class InOut : IInOut {
    public static readonly InOut Empty = new InOut(ImmutableList<IInOut>.Empty);

    public InOut(params IInOut[] elements) : this(elements.ToImmutableList()) {}

    public InOut(IEnumerable<IInOut> elements) : this(elements.ToImmutableList()) {}

    public InOut(IImmutableList<IInOut> elements) {
      Elements = elements;
    }

    public IImmutableList<IInOut> Elements { get; }

    public bool IsOkay => Elements.All(file => file.IsOkay);

    public InOut Add(IEnumerable<IInOut> ioElements)
      => new InOut(Elements.AddRange(ioElements));

    public static InOut Merge(params InOut[] inOuts) => Merge((IEnumerable<InOut>) inOuts);

    public static InOut Merge(IEnumerable<InOut> inOuts)
      => new InOut(inOuts.Aggregate(ImmutableList.CreateBuilder<IInOut>(), (builder, inOut) => {
        builder.AddRange(inOut.Elements);
        return builder;
      }).ToImmutable());

    protected bool Equals(InOut other) => Elements.SequenceEqual(other.Elements);

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) {
        return false;
      }
      if (ReferenceEquals(this, obj)) {
        return true;
      }
      return obj.GetType() == GetType() && Equals((InOut) obj);
    }

    public override int GetHashCode() => EnumerableUtils.ElementwiseHashCode(Elements);

    public static bool operator ==(InOut left, InOut right) => Equals(left, right);

    public static bool operator !=(InOut left, InOut right) => !Equals(left, right);

    public override string ToString() => $"InOut({string.Join(", ", Elements)})";
  }
}