__Table of contents__

* [About](#about)
* [Installing on Windows](#installing-on-windows)
* [Installing on Linux and OSX](#installing-on-linux-and-osx)
* [Quick start](#quick-start)
* [Main features](#main-features)
* [TODO](#todo)


## About

Bud is a build tool. Bud is currently in development. Release of version `1.0.0` is scheduled for June 2016.


## Installing on Windows

Install [Chocolatey](https://chocolatey.org/).

Install NuGet 3.3.0 or above (via chocolatey: `choco install nuget.commandline`).

Install Bud with this command (requires administrative rights):

```bash
$ choco upgrade bud -version 0.5.0-pre-3 -pre
```


## Installing on Linux and OSX

1. Install nuget v3 (must be on `PATH`).

1. Download the zip from https://dl.bintray.com/matej/bud/bud-0.5.0-pre-3.zip

1. Extract the zip somewhere, say in `$HOME/.bud/bud-0.5.0-pre-3`.

1. Invoke bud in the following way:

    ```bash
    $ mono $HOME/.bud/bud-0.5.0-pre-3/bud.exe <arguments>
    ```

__Warning__: Bud was not yet tested on Linux and OSX! Please let us know if you were unable to run bud on these platforms.

> TODO: Provide a shim script, called `bud`, that runs `bud.exe` with mono on Linux and OSX.
> TODO: provide a better installation method for Bud (perhaps just cloning from GitHub and running a script; similar to `rbenv`).


## Quick start

Create an empty folder (this will be the root of your solution). Let's call this folder `<root>`.

In the `<root>` folder, create the project folder, say `<root>/Your.Project`.

Create the file `<root>/Build.cs` with the following contents:

```csharp
using Bud.V1;
using static Bud.V1.Api;

public class BudBuild : IBuild {
  public Conf Init() => CsApp("Your.Project");
}
```

Create a "Hello World!" application. For example, create the file `Your.Project/App.cs` with
the following contents:

```csharp
namespace Your.Project {
  public class App {
    public static void Main(string[] args)
      => System.Console.WriteLine("Hello World!");
  }
}
```

Then go to the `<root>` folder, and invoke the following command:

```bash
bud Your.Project/Build
```

> TODO: Add automatic project generation (for even easier quickstart). Maybe via `bud -g CsApp Your.Project` or `bud -g CsLib Your.Project`

## Building

The following command builds continuously (until you press `Ctrl+C`):

```bash
$ bud Your.Project/Build
```

Bud will rebuild every time you change a source file.

## Publishing

The following command publishes your project to NuGet:

```bash
$ bud Your.Project/Publish
```


## Main features

- __Reactive__: bud will perform actions when files change.

- __Parallel__: tasks run in their own threads. Tasks push their output onto the build pipeline. This triggers dependent tasks.

- __Incremental__: Bud tracks which inputs have changed. For example, Bud tells [Roslyn](https://github.com/dotnet/roslyn) to re-parse only changed C# files.

- __Native REST API__ (TODO): Every task in Bud is defined through a REST-compliant resource string. For example, to get the dependencies of a project, you can issue an HTTP GET request on this resource `/MyProject/Dependencies`. Streams, like `/MyProject/Build` produce a WebSocket.


## TODO

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
