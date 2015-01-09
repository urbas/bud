# Bud

Bud is a build tool. The word _Bud_ stands for _Build without the 'ill'_.

## Goals

- Bud must compile sources under "src/test/cs" and produce a DLL out of that source.

- Bud must be able to run tests.

- Bud must build itself.

- Bud must have an interactive and non-interactive command-line mode.

- Bud must be able to build NuGet packages and publish them to NuGet repositories.

- Users should be able to write plugins, publish them via NuGet, and use them in their build definitions.

Optional:

- Bud must generate sln and csproj files from the build definition.