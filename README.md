# Bud

Bud is a build tool.

Bud is currently in development. The first stable version `1.0.0` will be released in June 2016.


## Main features

- __Reactive__: bud will perform actions when files change.

- __Parallel__: tasks run in their own threads. Tasks push their output onto the build pipeline. This triggers dependent tasks.

- __Incremental__: Bud tracks which inputs have changed. For example, Bud tells [Roslyn](https://github.com/dotnet/roslyn) to re-parse only changed C# files.

- __Native REST API__ (TODO): Every task in Bud is defined through a REST-compliant resource string. For example, to get the dependencies of a project, you can issue an HTTP GET request on this resource `/MyProject/Dependencies`. Streams, like `/MyProject/Build` produce a WebSocket.


## Prerequisites

__All platforms__:

- NuGet v3 must be installed and available on the `PATH`.

__Windows__:

- `choco` must be installed and available on the `PATH`. This is used to ppush distributions of applications to [Chocolatey](https://chocolatey.org/).

## Installation

__Windows__:

Invoke this command on the command line (requires administrative rights):

```bash
$ choco upgrade bud -version 0.5.0-pre-3 -pre
```

__Linux and OSX__:


1. Download the zip from: https://dl.bintray.com/matej/bud/bud-0.5.0-pre-3.zip

1. Extract the zip somewhere, say in `$HOME/.bud/bud-0.5.0-pre-3`.

1. Invoke bud in the following way:

    ```bash
    $ mono $HOME/.bud/bud-0.5.0-pre-3/bud.exe <arguments>
    ```

_Warning_: Bud was not yet tested on Linux and OSX! Please let us know if you were unable to run bud on these platforms.

> TODO: Provide a shim script, called `bud`, that runs `bud.exe` with mono on Linux and OSX.