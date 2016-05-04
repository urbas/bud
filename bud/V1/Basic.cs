using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Reactive;
using System.Reactive.Concurrency;
using Bud.Configuration;
using Bud.Util;

namespace Bud.V1 {
  public static class Basic {
    public const string BuildDirName = "build";
    public const string DefaultVersion = "0.0.1";

    /// <summary>
    ///   The directory relative to which all <see cref="BuildDir" /> paths are
    ///   calculated. By default this is the directory of the <c>Build.cs</c> file
    ///   that Bud is currently invoking. It can be overridden.
    /// </summary>
    /// <remarks>
    ///   By default the <see cref="ProjectDir" /> is located directly within this directory.
    /// </remarks>
    public static readonly Key<string> BaseDir = nameof(BaseDir);

    /// <summary>
    ///   A list of keys (paths) to other builds. For example, say we defined two projects
    ///   <c>A</c> and <c>B</c>. To make <c>B</c> depend on <c>A</c>, one would add the
    ///   <c>../A</c> to the list of <see cref="Dependencies" />.
    /// </summary>
    public static readonly Key<IImmutableSet<string>> Dependencies = nameof(Dependencies);

    /// <summary>
    ///   By default the entire build pipeline (input, sources, build, and output) are
    ///   scheduled on the same scheduler and the same thread (i.e.: the build pipeline
    ///   is single threaded). The build pipeline is also asynchronous. For example,
    ///   compilers can run each in their own thread and produce output when they finish.
    ///   The output is collected in the build pipeline's thread.
    /// </summary>
    /// <remarks>
    ///   You should never need to override this outside of testing. In all honesty, this
    ///   key is mostly meant for testing.
    /// </remarks>
    public static readonly Key<IScheduler> BuildPipelineScheduler = nameof(BuildPipelineScheduler);

    /// <summary>
    ///   The build's identifier. This identifier is used in <see cref="Dependencies" />.
    /// </summary>
    public static readonly Key<string> ProjectId = nameof(ProjectId);

    /// <summary>
    ///   The build's directory. Ideally, all the sources of this build
    ///   should be located within this directory.
    /// </summary>
    /// <remarks>
    ///   If this is a relative directory then Bud will combine it with
    ///   <see cref="BaseDir" />. If this is an absolute directory, Bud
    ///   will leave it unchanged.
    /// </remarks>
    public static readonly Key<string> ProjectDir = nameof(ProjectDir);

    /// <summary>
    ///   The directory where all outputs and generated files are placed.
    ///   This directory is by default deleted through the <see cref="Clean" />
    ///   command.
    /// </summary>
    public static readonly Key<string> BuildDir = nameof(BuildDir);

    /// <summary>
    ///   By default, deletes the entire <see cref="BuildDir" />
    /// </summary>
    public static readonly Key<Unit> Clean = nameof(Clean);

    /// <summary>
    ///   The version of the project. By default, it's <see cref="DefaultVersion" />.
    /// </summary>
    public static Key<string> ProjectVersion = nameof(ProjectVersion);

    private static readonly Lazy<EventLoopScheduler> DefauBuildPipelineScheduler
      = new Lazy<EventLoopScheduler>(() => new EventLoopScheduler());

    internal static readonly Conf BuildSchedulingSupport
      = BuildPipelineScheduler.Init(_ => DefauBuildPipelineScheduler.Value);


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
    /// <remarks>
    ///   This method delegates to <see cref="Project" />
    ///   it uses <paramref name="projectId" /> as both the project dir and
    ///   project ID.
    /// </remarks>
    public static Conf Project(string projectId,
                               Option<string> projectDir = default(Option<string>),
                               Option<string> baseDir = default(Option<string>))
      => BareProject(projectId, baseDir)
        .Add(BuildSchedulingSupport)
        .InitEmpty(Dependencies)
        .Init(ProjectDir, c => GetProjectDir(c, projectDir))
        .Init(ProjectVersion, DefaultVersion)
        .Init(BuildDir, DefaultBuildDir)
        .Init(Clean, DefaultClean);

    private static string GetProjectDir(IConf c, Option<string> projectDir)
      => projectDir.Map(dir => Path.Combine(BaseDir[c], dir))
                   .GetOrElse(() => Path.Combine(BaseDir[c], ProjectId[c]));

    private static Unit DefaultClean(IConf c) {
      var targetDir = BuildDir[c];
      if (Directory.Exists(targetDir)) {
        Directory.Delete(targetDir, true);
      }
      return Unit.Default;
    }

    private static string DefaultBuildDir(IConf c)
      => Path.Combine(BaseDir[c], BuildDirName, Path.GetDirectoryName(c.Key));

    /// <summary>
    ///   <para>
    ///     A bare project contains only the <see cref="ProjectId" /> and <see cref="BaseDir" /> configurations.
    ///   </para>
    ///   <para>
    ///     A project is a grouping of configurations. Configurations in this project will have the path of
    ///     <c>projectId/{conf key}</c>. You can add projects into projects. For example,
    ///     the code
    ///     <code>
    /// Project("A")
    ///   .Set(Foo, 42)
    ///   .Add(Project("B")
    ///          .Set(Foo, 9001))
    /// </code>
    ///     would create configurations <c>A/Foo</c> and <c>A/B/Foo</c>.
    ///   </para>
    /// </summary>
    /// <param name="projectId">see <see cref="ProjectId" />.</param>
    /// <param name="baseDir">
    ///   <para>
    ///     The directory under which all projects should live. By default this is the directory
    ///     where the <c>Build.cs</c> script is located.
    ///   </para>
    ///   <para>
    ///     By default this is where the <see cref="BuildDir" /> will be located.
    ///   </para>
    /// </param>
    /// <returns>a bag of configurations.</returns>
    public static Conf BareProject(string projectId,
                                   Option<string> baseDir = default(Option<string>)) {
      if (string.IsNullOrEmpty(projectId)) {
        throw new ArgumentNullException(nameof(projectId), "A project's ID must not be null or empty.");
      }
      if (projectId.Contains("/")) {
        throw new ArgumentException($"Project ID '{projectId}' is invalid. It must not contain the character '/'.", nameof(projectId));
      }
      return Conf.Group(projectId)
                 .Init(ProjectId, projectId)
                 .Init(BaseDir, c => baseDir.OrElse(() => c.TryGet(".."/BaseDir))
                                            .GetOrElse(() => {
                                              throw new Exception("Could not determine the base directory.");
                                            }));
    }

    public static Conf Projects(params IConfBuilder[] confs)
      => Conf.Group((IEnumerable<IConfBuilder>) confs);

    public static Conf Projects(IEnumerable<IConfBuilder> confs)
      => Conf.Group(confs);
  }
}