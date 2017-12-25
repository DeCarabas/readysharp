namespace ReadyGo {
  using System;
  using System.Diagnostics;
  using System.Runtime.CompilerServices;

  sealed class NullBenchmark : Benchmark {
    public override string Name => "(null)";

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Setup() {
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Cleanup() {
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Go() {
    }
  }

  public class Runner {
    const int OuterIterations = 16;
    static readonly TimeSpan MinimumTime = TimeSpan.FromMilliseconds(3);
    static readonly Benchmark NullBenchmark = new NullBenchmark();
    readonly Benchmark[] benchmarks;

    public Runner(Benchmark[] benchmark) {
      this.benchmarks = benchmark;
    }

    static TimeSpan MeasureRuntime(int iterations, Benchmark benchmark) {
      var sw = new Stopwatch();

      GC.Collect();
      sw.Start();
      for (int i = 0; i < iterations; i++) {
        benchmark.Go();
      }
      sw.Stop();
      return sw.Elapsed;
    }

    static TimeSpan RunBenchmark(Benchmark benchmark, int iterations) {
      TimeSpan rawTime = MeasureRuntime(iterations, benchmark);
      TimeSpan constantTime = MeasureRuntime(iterations, NullBenchmark);

      return rawTime - constantTime;
    }

    static void Prime(Benchmark benchmark) {
      benchmark.Setup();
      benchmark.Go();
      benchmark.Cleanup();
    }

    static TimeSpan CaptureTime(
      Benchmark benchmark,
      int iterations,
      bool throwOnTooFast = false) {
      benchmark.Setup();
      TimeSpan result = RunBenchmark(benchmark, iterations);
      benchmark.Cleanup();

      if (throwOnTooFast) {
        if (result < MinimumTime) {
          throw new BenchmarkTooFastException();
        }
      }

      return TimeSpan.FromMilliseconds(result.TotalMilliseconds / iterations);
    }

    static TimeSpan Percentile(TimeSpan[] times, int percentile) {
      TimeSpan[] sortedTimes = new TimeSpan[times.Length];
      Array.Copy(times, sortedTimes, times.Length);
      Array.Sort(sortedTimes);

      double ratio = percentile * 0.01;
      double h = ratio * ((double)(sortedTimes.Length - 1)) + 1.0;
      int k = (int)(Math.Floor(h) - 1.0);
      double f = h % 1.0;

      return TimeSpan.FromMilliseconds(sortedTimes[k].TotalMilliseconds +
          (f * (sortedTimes[k + 1].TotalMilliseconds - sortedTimes[k].TotalMilliseconds)));
    }

    public BenchmarkResult[] Run() {
      int[] iterations = new int[this.benchmarks.Length];
      TimeSpan[][] benchTimes = new TimeSpan[this.benchmarks.Length][];
      for (int i = 0; i < this.benchmarks.Length; i++) {
        benchTimes[i] = new TimeSpan[OuterIterations];
      }
      for (int i = 0; i < this.benchmarks.Length; i++) {
        Prime(this.benchmarks[i]);
      }

      for (int i = 0; i < this.benchmarks.Length; i++) {
        Benchmark benchmark = this.benchmarks[i];
        TimeSpan[] times = benchTimes[i];

        iterations[i] = 1;
        while (true) {
          try {
            times[0] = CaptureTime(
              benchmark,
              iterations[i],
              throwOnTooFast: true);
            break;
          } catch (BenchmarkTooFastException) {
            Console.Write("!");
            iterations[i] *= 2;
            continue;
          }
        }
        Console.Write(".");
      }

      for (int i = 1; i < OuterIterations; i++) {
        for (int j = 0; j < this.benchmarks.Length; j++) {
          Benchmark benchmark = this.benchmarks[j];
          TimeSpan[] times = benchTimes[j];

          times[i] = CaptureTime(benchmark, iterations[j]);
          Console.Write(".");
        }
      }
      Console.WriteLine();

      var results = new BenchmarkResult[this.benchmarks.Length];
      for (int i = 0; i < this.benchmarks.Length; i++) {
        Benchmark benchmark = this.benchmarks[i];
        TimeSpan[] times = benchTimes[i];
        Array.Sort(times);

        results[i] = new BenchmarkResult(
          benchmark.Name,
          times[0],
          Percentile(times, 80));
      }
      return results;
    }

    class BenchmarkTooFastException : Exception {
    }
  }
}