Simply create a pull request and write a bit of a description. 


# Prerequisites

- Install `bud` (see [installation instructions](./README.md#installation))
- Install NuGet (make sure it's available on PATH)


## Windows

- Install Visual Studio 2015 or later.


## Linux and OSX

- Install latest version of `mono`.

# Building

```bash
bud bud/Compile
```

# Testing

Install NUnit.

Build the `bud.sln` solution.

Test the `Bud.Test` project with NUnit.

> TODO: Implement testing in Bud.


# Performance measurement

```bash
bud benchmark
```

This will produce the file `build/benchmarks/benchmark-results.json` and push it to [BinTray](https://bintray.com/matej/bud/bud-benchmarks/view#files).


# Releasing

Publishes to NuGet:

```bash
bud bud/Publish bud/Distribute
```