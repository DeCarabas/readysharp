# ReadySharp

This is a benchmarking tool that is basically a direct clone of Gary Bernhardt's [ReadyGo](https://github.com/garybernhardt/readygo) tool for ruby. 
In particular, it follows the same [timing methodology](https://github.com/garybernhardt/readygo#timing-methodology) as that tool, and it has similar properties:

* It goes to great lengths to produce accurate numbers by nulling out sources of timing offsets and jitter.

* It can reliably measure aggregate GC costs to sub-nanosecond accuracy (that's less than one CPU cycle per benchmark run).

* It's pretty fast if your benchmark is fast.
For a benchmark taking about 1ms, a full run will take around half a second.

* It draws text plots right at the terminal so your flow isn't broken.

* It records benchmark runtime, stores it in a file, and compares it to benchmark runs that you make after changing the code.
Most benchmarking tools are designed for comparison of multiple alternatives in the same source tree, which usually requires more effort.

It is a little different in the details, though:

* It requires that each benchmark be in a class, not in a set of blocks.

* It only supports one benchmark method per `Setup`/`Cleanup` pair.
Use inheritance to share `Setup`/`Cleanup` methods.

* It is designed to be hosted from within a program, rather than loading and running the program from the outside.
This provides a little more flexibility in the choice of runtime environment, at the cost of a little more complexity.

## Basic Usage

Benchmarks are defined as classes implementing the `IBenchmark` interface:

```C#
using System.IO;
class BigFiles : IBenchmark 
{
    public string Name => "big files";
    public void Setup() => File.WriteAllText("foo", new String('X', 1000000));
    public void Cleanup() => File.Delete("foo");
    public void Go() => File.ReadAllText("foo");
}
```

The `Setup` and `Cleanup` methods do setup and teardown.
Their cost is not included in the reported benchmarks.
The `Go` method contians the code to be measured.

Benchmarks are executed by calling the `Ready.Go` method, usually from the 
`Main` method of a command-line EXE. For example:

```C#
using ReadyGo;
static class Program
{
    public static int Main(string[] args) => Ready.Go(args, new BigFiles());
}
```

To record the system's current performance as a baseline, run:

```
dotnet run -- --record
```

The recorded timings are written to a JSON file called `.readysharp` and will serve as a baseline for subsequent runs.
To make a benchmark comparison against other branches, changes that you make, etc., run:

```
dotnet run -- --compare
```

This will load the recorded baseline benchmark numbers, re-run the benchmarks against the current code, and compare.
It will look something like this:

```
!................
big files
  Baseline: |                                     x----------------------------|
  Current:  |                                     x-------------------         |
            0                                                            3.50 ms
```

The line of dots and bangs is a progress indicator to let you know that it's still alive.
Dots represent benchmarks being run; bangs represent benchmarks that were too fast and were wrapped in loops automatically to increase their runtime (see [Timing Methodology](#timing-methodology) for more on that).
Times are expressed in seconds (s), milliseconds (ms), microseconds (us), or nanoseconds (ns) as appropriate.

The plot shows a rough visual indication of performance differences between the last recording ("Baseline") and the current system ("Current").
It provides the best estimate of actual runtime cost (the X, which is the lowest sampled runtime), as well as a visual indication of the variance (the bar to the right of the X, which extends until the 80th percentile of runtime).

## Usage Tips

* Accuracy will be best when nothing else is running on the machine, especially high-load processes like backups.
This is true for any benchmarking tool.

* Implementing multiple variations of a piece of code is rarely necessary.
Instead, `dotnet run -- --record` the current benchmark performance, make your changes to the code (or switch branches), then `dotnet run -- --compare` to see how your changes affected the benchmarks.

* Move as much as possible into `Setup` and `Cleanup` methods.
Generally, the `Go` method should be a single method call.
This ensures that you're benchmarking only one thing.