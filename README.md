[Installation](#installation)
- [Windows](#windows)
- [Linux and OSX](#linux-and-osx)
[Main features](#main-features)

# Bud

Bud is a build tool.

Bud is currently in development. Release of version `1.0.0` is scheduled for June 2016.


## Installation

- NuGet v3 must be installed and available on the `PATH`.


### Windows

Install [Chocolatey](https://chocolatey.org/).

Install NuGet v3.

Invoke this command (requires administrative rights):

```bash
$ choco upgrade bud -version 0.5.0-pre-3 -pre
```


### Linux and OSX

1. Install nuget v3.

1. Download the zip from: https://dl.bintray.com/matej/bud/bud-0.5.0-pre-3.zip

1. Extract the zip somewhere, say in `$HOME/.bud/bud-0.5.0-pre-3`.

1. Invoke bud in the following way:

    ```bash
    $ mono $HOME/.bud/bud-0.5.0-pre-3/bud.exe <arguments>
    ```

__Warning__: Bud was not yet tested on Linux and OSX! Please let us know if you were unable to run bud on these platforms.

> TODO: Provide a shim script, called `bud`, that runs `bud.exe` with mono on Linux and OSX.


## Main features

- __Reactive__: bud will perform actions when files change.

- __Parallel__: tasks run in their own threads. Tasks push their output onto the build pipeline. This triggers dependent tasks.

- __Incremental__: Bud tracks which inputs have changed. For example, Bud tells [Roslyn](https://github.com/dotnet/roslyn) to re-parse only changed C# files.

- __Native REST API__ (TODO): Every task in Bud is defined through a REST-compliant resource string. For example, to get the dependencies of a project, you can issue an HTTP GET request on this resource `/MyProject/Dependencies`. Streams, like `/MyProject/Build` produce a WebSocket.