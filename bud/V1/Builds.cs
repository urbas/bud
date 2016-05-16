using System;
using System.Collections.Immutable;
using System.Reactive.Linq;
using Bud.Collections;
using Bud.Reactive;
using Bud.Util;
using static Bud.V1.Basic;

namespace Bud.V1 {
  public static class Builds {
    /// <summary>
    ///   An observable of lists of files. This observable should
    ///   produce a new element every time the input changes.
    /// </summary>
    public static readonly Key<IObservable<IImmutableList<string>>> Input = nameof(Input);

    /// <summary>
    ///   By default, output produces a single empty list of files.
    /// </summary>
    public static readonly Key<IObservable<IImmutableList<string>>> Output = nameof(Output);

    /// <summary>
    ///   This observable stream contains aggregated output from all <see cref="Dependencies"/>
    /// </summary>
    public static readonly Key<IObservable<IImmutableList<string>>> DependenciesOutput = nameof(DependenciesOutput);

    /// <param name="projectId">see <see cref="ProjectId" />.</param>
    /// <param name="projectDir">
    ///   This is the directory in which all sources of this project will live.
    ///   <para>
    ///     If none given, the <see cref="ProjectDir" /> will be <see cref="BaseDir" /> appended with the
    ///     <see cref="ProjectId" />.
    ///   </para>
    ///   <para>
    ///     If the given path is relative, then the absolute <see cref="ProjectDir" /> will
    ///     be resolved from the <see cref="BaseDir" />. Note that the <paramref name="projectDir" />
    ///     can be empty.
    ///   </para>
    ///   <para>
    ///     If the given path is absolute, the absolute path will be taken verbatim.
    ///   </para>
    /// </param>
    /// <param name="baseDir">
    ///   <para>
    ///     The directory under which all projects should live. By default this is the directory
    ///     where the <c>Build.cs</c> script is located.
    ///   </para>
    ///   <para>
    ///     By default this is where the <see cref="BuildDir" /> will be located.
    ///   </para>
    /// </param>
    public static Conf BuildProject(string projectId,
                                    Option<string> projectDir = default(Option<string>),
                                    Option<string> baseDir = default(Option<string>))
      => Project(projectId, projectDir, baseDir)
      .InitEmpty(Input)
      .InitEmpty(Output)
      .Init(DependenciesOutput, GatherOutputsFromDependencies);

    private static IObservable<IImmutableList<string>> GatherOutputsFromDependencies(IConf c)
      => Dependencies[c]
        .Gather(dependency => c.TryGet(dependency/Output))
        .Combined()
        .Select(ImmutableLists.FlattenToImmutableList);
  }
}