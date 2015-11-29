using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bud.Collections;

namespace Bud.IO {
  public class InOut {
    public static readonly InOut Empty = new InOut(ImmutableList<InOutFile>.Empty);

    public InOut(ImmutableList<InOutFile> files) {
      Files = files;
    }

    public ImmutableList<InOutFile> Files { get; }

    public bool IsOkay => Files.TrueForAll(file => file.IsOkay);

    public InOut AddFiles(params string[] files) => AddFiles((IEnumerable<string>)files);

    public InOut AddFiles(IEnumerable<string> files) => new InOut(Files.AddRange(files.Select(InOutFile.Create)));

    public static InOut Create(params string[] files) => Create((IEnumerable<string>) files);

    public static InOut Create(IEnumerable<string> files) => new InOut(files.Select(InOutFile.Create).ToImmutableList());

    public static InOut Create(string file, bool isOkay)
      => new InOut(ImmutableList.Create(InOutFile.Create(file, isOkay)));

    public static InOut Merge(params InOut[] inOuts) => Merge((IEnumerable<InOut>) inOuts);

    public static InOut Merge(IEnumerable<InOut> inOuts)
      => new InOut(inOuts.Aggregate(ImmutableList.CreateBuilder<InOutFile>(), (builder, inOut) => {
        builder.AddRange(inOut.Files);
        return builder;
      }).ToImmutable());

    protected bool Equals(InOut other) => Files.SequenceEqual(other.Files);

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) {
        return false;
      }
      if (ReferenceEquals(this, obj)) {
        return true;
      }
      return obj.GetType() == GetType() && Equals((InOut) obj);
    }

    public override int GetHashCode() => EnumerableUtils.ElementwiseHashCode(Files);

    public static bool operator ==(InOut left, InOut right) => Equals(left, right);

    public static bool operator !=(InOut left, InOut right) => !Equals(left, right);

    public override string ToString() => $"InOut({string.Join(", ", Files)})";
  }
}