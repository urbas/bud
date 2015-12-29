# Bud

Bud is a build tool.

Bud is currently in development. The first release is planned for June 2016.

## Prerequisites

- NuGet v3 must be installed and in the `PATH`.

## Main features

- __Reactive__: inputs and outputs in Bud are reactive streams. For example, builds are re-triggered whenever an input changes (e.g.: a source file on the filesystem or an HTTP resource).

- __Parallel__: projects are built in their own threads. They push their output onto the build pipeline. Dependent projects simply react and rebuild themselves.

- __Incremental__: Bud is aware of which inputs have changed and whether the outputs are out of date. Bud's prime example of incremental compilation is the [Roslyn compiler](https://github.com/dotnet/roslyn). See [this video](https://www.youtube.com/watch?v=Lkx0-2l2V7w) for a demonstration of incremental compilation with Bud and Roslyn.

- __Native REST API__ (TODO): Every task and resource in Bud is defined through a REST-compliant resource string. For example, to get the dependencies of a project, you can issue an HTTP GET request on this resource `/MyProject/Dependencies`. Streams, like `/MyProject/Build` produce a WebSocket.
