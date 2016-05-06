using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Bud.Cs;
using Bud.IO;
using Bud.NuGet;
using Bud.Util;

namespace Bud.V1 {
  /// <summary>
  ///   Defines the core concepts of every build in Bud.
  ///   <para>
  ///     Every build has an ID and a directory.
  ///   </para>
  ///   <para>
  ///     In addition, every build has three observable streams: input, build, and output.
  ///     The input is piped (unmodified) through to the build and then frurther
  ///     through to output.
  ///   </para>
  ///   <para>
  ///     The build is defined entirely through keys defined in this class. For example,
  ///     the input, build, and output are defined with keys <see cref="Builds.Input" />,
  ///     <see cref="Builds.Build" />, and <see cref="Conf.Out" />. One can customise these through
  ///     the <see cref="Conf" /> API (such as the <see cref="Conf.Modify{T}" /> method).
  ///   </para>
  /// </summary>
  public static class Api {
    #region Distribution Support

    /// <summary>
    ///   Returns a list of files to package. These file will end up in
    ///   the archive at <see cref="DistributionArchivePath" /> produced by
    ///   <see cref="DistributionArchive" />.
    /// </summary>
    public static readonly Key<IObservable<IImmutableList<PackageFile>>> FilesToDistribute = nameof(FilesToDistribute);

    /// <summary>
    ///   The path where <see cref="DistributionArchive" /> should place the archive.
    /// </summary>
    public static readonly Key<string> DistributionArchivePath = nameof(DistributionArchivePath);

    /// <summary>
    ///   Creates an archive that contains all that is needed for the
    ///   distribution of the project. It returns the path to the created
    ///   archive.
    /// </summary>
    public static readonly Key<IObservable<string>> DistributionArchive = nameof(DistributionArchive);

    /// <summary>
    ///   Pushes the project to a distribution channel. The default implementation places
    ///   the <see cref="DistributionArchive" /> into BinTray, uploads a Chocolatey
    ///   package to the Chocolatey page, and returns <c>true</c> if the operation
    ///   succeeded.
    /// </summary>
    public static readonly Key<IObservable<bool>> Distribute = nameof(Distribute);

    /// <summary>
    ///   Provides the <see cref="DistributionArchive" /> task, which produces
    ///   a distributable archive. The default implementation of the distribution
    ///   produces a ZIP archive in the <see cref="DistributionArchivePath" />. This path
    ///   is not set by default, you have to set it to the desired value.
    ///   <para>
    ///     Add files to the <see cref="FilesToDistribute" /> list in order to include them
    ///     in the produced ZIP archive.
    ///   </para>
    /// </summary>
    public static Conf DistributionSupport => Dist.ProjectDistribution.DistributionSupport;

    #endregion
  }
}