# Bud

Bud is a build tool. The word _Bud_ stands for _Build without the 'ill'_.

## Goals

- Bud must be able to build NuGet packages and publish them to NuGet repositories.

- Bud must be able to run tests.

- Bud must have an interactive and non-interactive command-line mode.

- Users should be able to write plugins, publish them via NuGet, and use them in their build definitions.

Optional:

- Bud must generate sln and csproj files from the build definition.

## Build structure

### Projects

A build consists of multiple projects.

Key: `project/:projectId`.

### Project configuration

Each project has a number of configured properties, such as the base directory.

Key: `Project/baseDir`.

### Build targets

A project contains multiple build targets. A build target is defined by two keys: the build scope and the language.

Key: `Project/:scope/:language`.

Examples: `project/foo/main/cs`, `project/foo/test/cs`, `project/foo/main/java`.

## Global tasks

### `build`

Builds all build targets in all projects.

### `test`

Runs tests for all projects and all languages.