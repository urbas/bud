# TODO List

- Performance benchmarks:

    - Startup time (compiling `Build.cs`; loading it after it is built).

    - Compiling variously-sized projects (check a project out from git, cold-compile it, warm-compile it, hot-compile it, clean-compile it).

- Tests for API-breaking changes:

    - Add this feature to Bud (so that everyone can do backwards-compatibility tests).

    - Add backwards-compatibility checks to Bud's build itself.

- Incremental compilation:

    - Add the ability to use hashes instead of timestamps for incremental compilation.

    - Check if Roslyn can compile files separately and implement true incremental compilation.

- Use NuGet instead of `choco` in `ChocoDistribution.cs`.

- I want some help in `README.md`. After 1 minute, I should be able to build my project.

- Add Linux support.

    - Run tests on TravisCI or Appveryor.

- Add OSX support.

    - Any OSX-based CI services? Get a Mac?

- Improved CLI Logging. Example: invoking `Bud.Test/Compile`

        21:15:45.000> Bud/Packages/Resolve
        21:15:48.000< Bud/Packages/Resolve [3000ms]
        21:15:48.000> Bud/Compile
        21:15:48.250< Bud/Compile [250ms]
        21:15:48.250> Bud.Test/Packages/Resolve
        21:15:49.000< Bud.Test/Packages/Resolve [750ms]
        21:15:49.000> Bud.Test/Compile
        21:15:49.300< Bud.Test/Compile [300ms]

- I want to use Bud in Visual Source.

    - Bud should communicate to Visual Source through HTTP. The protocol:

        - GET for `IObservable` keys takes 1, serializes to JSON, and returns it.

        - GET for `Task` keys waits and return JSON-serialized result.

        - GET for everything else simply returns a JSON-serialized result.

        - WebSockets are available only for `IObservable` keys.

- Compile projects in parallel.

- I want to test my projects.

- Generate `csproj` files for projects.

- I want to generate MSBuild solution and project files for my projects.

- I want to define a generic build which doesn't rebuild if sources are up-to-date with outputs.

    [Partial solution] Consider using `HashBasedCaching.GetLinesOrCache`.

- Use less than 10ms to start a simple build.

## Postponed

- I want to clean my projects without cleaning the resolved packages.

  - [Workaround] You can define a top-level task that does what you want:

        Clean.Init(c => {
          c.TryGet("Bud"/Clean);
          c.TryGet("Bud.Test"/Clean);
          return Unit.Default;
        })

  - [Proposal 1] We can introduce an 'Alias(Key, ...)' function to be used like this: `Alias(Clean, "Bud/Clean", "Bud.Test/Clean")`. One can then define arbitrary aliases that suit the needs of the developer.

  - [Proposal 2] We could allow wildcard keys, such as `bud */Clean` and `bud **/Clean`. (Partially implemented in git history: 195bde314a1e717143dac800a733efbcadcd3b86)

- Alias `bud Command` to `bud **/Command`. Do not alias absolute paths like `bud /Command` or wildcarded paths like `bud */Command` and `bud a/**/b`.

    - [Workaround] define a top-level command that you want to alias.