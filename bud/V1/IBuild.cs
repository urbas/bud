namespace Bud.V1 {
  /// <remarks>
  ///   <para>How to use this interface?</para>
  ///   <para>
  ///     Create a file called <c>Build.cs</c>. In this file,
  ///     create a class that implements this interface.
  ///     Implement the <see cref="Init()" /> method. This method
  ///     must return a <see cref="Conf" /> object. The returned
  ///     object is a description of your build.  See Bud's
  ///     own <c>Build.cs</c> file for an example of a build configuration:
  ///     https://github.com/urbas/bud/
  ///   </para>
  /// </remarks>
  public interface IBuild {
    Conf Init();
  }
}